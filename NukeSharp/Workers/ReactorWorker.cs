using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NukeSharp.Simulator;

namespace NukeSharp.Workers;

internal class ReactorWorker(IReactor reactor, ILogger<ReactorWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await reactor.Start(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Reactor task was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred in the reactor.");
        }
    }
}
