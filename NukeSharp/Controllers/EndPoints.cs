using HandlebarsDotNet;
using System.Text.Json;
using NukeSharp.Models;
using NukeSharp.Services;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;

namespace NukeSharp.Controllers;

public class EndPoints(IPressureSensor pressureSensor, ILogger<EndPoints> logger)
{
    private readonly HandlebarsTemplate<object, object> _indexTemplate = Handlebars.Compile(File.ReadAllText("static/index.html"));
    private readonly HandlebarsTemplate<object, object> _pressureTemplate = Handlebars.Compile(File.ReadAllText("static/pressure.handlebars"));


    private readonly ConcurrentQueue<float> _last100Readings = new();

    public async Task GetIndex(HttpContext context)
    {
        var result = _indexTemplate(null);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(result);
    }

    public async Task GetCurrentPressure(HttpContext context)
    {
        var time = DateTime.UtcNow;
        var pressure = pressureSensor.GetValue();
        if (_last100Readings.Count >= 100)
        {
            _last100Readings.TryDequeue(out _);
        }
        logger.LogInformation("GetValue from controller at {time}. Current pressure: {pressure}", time, pressure);
        _last100Readings.Enqueue(pressure);
        var p = new PressureResult { Pressure = pressure.ToString() };
        context.Response.ContentType = "text/html";
        var result = _pressureTemplate(p);
        await context.Response.WriteAsync(result);
    }

    public async Task GetHistoricMeasurements(HttpContext context)
    {
        var json = JsonSerializer.Serialize(_last100Readings.ToArray());

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(json);
    }
}