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

        app.MapGet("/", Get(indexTemplate));

        app.MapGet("/pressure", GetPressure(pressureTemplate));

        app.MapGet("/gethistoricmeasurements", GetHistoric());

        app.MapPost("/thresholds", SetThresholds());
    }

    private static Func<HttpContext, ReactorSystem, System.Threading.Tasks.Task> SetThresholds()
    {
        return async (context, reactorSystem) =>
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
                await context.Response.WriteAsync("Open threshold must be above close threshold");
                return;
            }

            reactorSystem.SetThresholds(openAt, closeAt);
            await context.Response.WriteAsync("Updated");
        };
    }

    private static Func<HttpContext, PressureHistory, System.Threading.Tasks.Task> GetHistoric()
    {
        return static async (context, history) =>
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(history.GetAll()));
        };
    }

    private static Func<
        HttpContext,
        IPressureSensor,
        PressureHistory,
        ILogger<PressureHistory>,
        System.Threading.Tasks.Task
    > GetPressure(HandlebarsTemplate<object, object> pressureTemplate)
    {
        return async (context, pressureSensor, history, logger) =>
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
        };
    }

    private static Func<HttpContext, ReactorSystem, System.Threading.Tasks.Task> Get(
        HandlebarsTemplate<object, object> indexTemplate
    )
    {
        return async (context, reactorSystem) =>
        {
            string result = indexTemplate(
                new { openAt = reactorSystem.MaxPressure, closeAt = reactorSystem.MinPressure }
            );
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(result);
        };
    }
}
