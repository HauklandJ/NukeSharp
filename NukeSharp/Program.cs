using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using NukeSharp.Controllers;
using NukeSharp.ControlSystem;
using NukeSharp.Services;
using NukeSharp.Simulator;
using NukeSharp.Workers;


IConfigurationBuilder congifBuilder = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json");

IConfigurationRoot configuration = congifBuilder.Build();

string pressureDeviationType = configuration["PressureDeviationType"] ?? "standard";


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddSingleton<ReactorSystem>();
builder.Services.AddHostedService<ReactorWorker>();

WebApplication app = builder.Build();
app.Services.GetRequiredService<ReactorSystem>();

IPressureSensor pressureSensor = app.Services.GetRequiredService<IPressureSensor>();
ILogger<EndPoints> logger = app.Services.GetRequiredService<ILogger<EndPoints>>();
EndPoints endpoints = new(pressureSensor, logger);

app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "static")), RequestPath = "/static" });

app.MapGet("/", endpoints.GetIndex);
app.MapGet("/pressure", endpoints.GetCurrentPressure);
app.MapGet("/gethistoricmeasurements", endpoints.GetHistoricMeasurements);
await app.RunAsync();