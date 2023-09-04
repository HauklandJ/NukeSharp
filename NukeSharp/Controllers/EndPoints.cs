using HandlebarsDotNet;
using Newtonsoft.Json;
using NukeSharp.Models;
using NukeSharp.Services;

namespace NukeSharp.Controllers;

public class EndPoints
{
    private readonly IPressureSensor _pressureSensor;

    private readonly HandlebarsTemplate<object, object> _indexTemplate;
    private readonly HandlebarsTemplate<object, object> _pressureTemplate;


    private readonly Queue<float> _last100Readings = new(100);

    public EndPoints(IPressureSensor pressureSensor)
    {
        _pressureSensor = pressureSensor;

        _indexTemplate = Handlebars.Compile(File.ReadAllText("static/index.html"));
        _pressureTemplate = Handlebars.Compile(File.ReadAllText("static/pressure.handlebars"));
    }

    public async Task GetIndex(HttpContext context)
    {
        var result = _indexTemplate(null);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(result);
    }

    public async Task GetCurrentPressure(HttpContext context)
    {
        var time = DateTime.UtcNow;
        var pressure = _pressureSensor.GetValue();
        if (_last100Readings.Count >= 100)
        {
            _last100Readings.Dequeue();
        }
        Console.WriteLine($"GetValue from controller at {time}. Current pressure: {pressure}");
        _last100Readings.Enqueue(pressure);
        var p = new PressureResult { Pressure = pressure.ToString() };
        context.Response.ContentType = "text/html";
        var result = _pressureTemplate(p);
        await context.Response.WriteAsync(result);
    }

    public async Task GetHistoricMeasurements(HttpContext context)
    {
        var json = JsonConvert.SerializeObject(_last100Readings.ToArray());

        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(json);
    }
}