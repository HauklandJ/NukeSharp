using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NukeSharp;
using NukeSharp.ControlSystem;
using NukeSharp.Services;
using NukeSharp.Simulator;
using NukeSharp.Workers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string pressureDeviationType = builder.Configuration["PressureDeviationType"] ?? "standard";

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
    throw new InvalidOperationException(
        $"Invalid value for environment variable 'PressureDeviationType'. Expected 'Standard' or 'Randomized', but got '{pressureDeviationType}'."
    );
}

builder.Services.AddSingleton<IReactor, Reactor>();
builder.Services.AddSingleton<ReactorSystem>();
builder.Services.AddSingleton<PressureHistory>();
builder.Services.AddHostedService<ReactorWorker>();

WebApplication app = builder.Build();
app.Services.GetRequiredService<ReactorSystem>();

app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "static")
        ),
        RequestPath = "/static",
    }
);

app.MapPressureEndpoints();
await app.RunAsync();
