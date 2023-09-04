using Microsoft.Extensions.FileProviders;
using NukeSharp.Controllers;
using NukeSharp.ControlSystem;
using NukeSharp.Services;
using NukeSharp.Simulator;
using NukeSharp.Workers;


var congifBuilder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json");

var configuration = congifBuilder.Build();

string pressureDeviationType = configuration["PressureDeviationType"] ?? "standard";


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IValveControl, ValveControl>();

if (pressureDeviationType == "standard")
{
    builder.Services.AddSingleton<IPressureSensor, PressureSensor>();
} 
else if (pressureDeviationType == "randomized")
{
    builder.Services.AddSingleton<IPressureSensor, RandomizedPressureSensor>();
}
else 
{
    throw new InvalidOperationException($"Invalid value for environment variable 'PressureDeviationType'. Expected 'Standard' or 'Randomized', but got '{pressureDeviationType}'.");
}


builder.Services.AddSingleton<IReactor, Reactor>();
builder.Services.AddSingleton<IReactorSystem, ReactorSystem>();
builder.Services.AddHostedService<SystemWorker>();
builder.Services.AddHostedService<ReactorWorker>();

var app = builder.Build();

var pressureSensor = app.Services.GetRequiredService<IPressureSensor>();
var endpoints = new EndPoints(pressureSensor);

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "static")), RequestPath = "/static" });

app.MapGet("/", endpoints.GetIndex);
app.MapGet("/pressure", endpoints.GetCurrentPressure);
app.MapGet("/gethistoricmeasurements", endpoints.GetHistoricMeasurements);
await app.RunAsync();