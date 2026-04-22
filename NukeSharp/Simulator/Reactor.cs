using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NukeSharp.Services;

namespace NukeSharp.Simulator;

public class Reactor(IPressureSensor sensor, IValveControl valve, ILogger<Reactor> logger) : IReactor
{
    public async Task Start(CancellationToken cancellationToken)
    {
        logger.LogInformation("Started reactor");
        while (!cancellationToken.IsCancellationRequested)
        {
            bool isOpen = valve.IsOpen();
            await Task.Delay(1000, cancellationToken);
            sensor.Update(isOpen);
        }
    }
}