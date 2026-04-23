using System;
using System.IO;
using System.Text.Json;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NukeSharp.ControlSystem;
using NukeSharp.Models;
using NukeSharp.Services;

namespace NukeSharp;

public static class PressureEndpoints
{
    public static void MapPressureEndpoints(this WebApplication app)
    {
        var indexTemplate = Handlebars.Compile(File.ReadAllText("static/index.html"));
        var pressureTemplate = Handlebars.Compile(File.ReadAllText("static/pressure.handlebars"));

        app.MapGet(
            "/",
            async (HttpContext context, ReactorSystem reactorSystem) =>
            {
                string result = indexTemplate(
                    new { openAt = reactorSystem.MaxPressure, closeAt = reactorSystem.MinPressure }
                );
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(result);
            }
        );

        app.MapGet(
            "/pressure",
            async (
                HttpContext context,
                IPressureSensor pressureSensor,
                PressureHistory history,
                ILogger<PressureHistory> logger
            ) =>
            {
                float pressure = pressureSensor.GetValue();
                history.Record(pressure);
                logger.LogDebug(
                    "GetValue at {time}. Current pressure: {pressure}",
                    DateTime.UtcNow,
                    pressure
                );
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(
                    pressureTemplate(new PressureResult { Pressure = pressure.ToString() })
                );
            }
        );

        app.MapGet(
            "/gethistoricmeasurements",
            async (HttpContext context, PressureHistory history) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(history.GetAll()));
            }
        );

        app.MapPost(
            "/thresholds",
            async (HttpContext context, ReactorSystem reactorSystem) =>
            {
                if (
                    !float.TryParse(context.Request.Form["openAt"], out float openAt)
                    || !float.TryParse(context.Request.Form["closeAt"], out float closeAt)
                )
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid values");
                    return;
                }

                if (openAt <= closeAt)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync(
                        "Open threshold must be above close threshold"
                    );
                    return;
                }

                reactorSystem.SetThresholds(openAt, closeAt);
                await context.Response.WriteAsync("Updated");
            }
        );
    }
}
