using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NukeSharp.Simulator;

namespace NukeSharp.Workers;
internal class ReactorWorker(IReactor reactor, ILogger<ReactorWorker> logger) : IHostedService
{
    private readonly CancellationTokenSource _shutdownTokenSource = new();
    private Task _asyncReactor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownTokenSource.Token);

        _asyncReactor = Task.Run(() => reactor.Start(linkedTokenSource.Token), linkedTokenSource.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdownTokenSource.Cancel();

        try
        {
            await _asyncReactor;
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Reactor task was canceled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while stopping the reactor.");
        }
    }

}
