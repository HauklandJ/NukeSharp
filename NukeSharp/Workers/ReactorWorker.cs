using NukeSharp.Simulator;

namespace NukeSharp.Workers;
internal class ReactorWorker : IHostedService
{
    private readonly IReactor _reactor;
    private readonly CancellationTokenSource _shutdownTokenSource = new();
    private readonly ILogger<ReactorWorker> _logger;

    public ReactorWorker(IReactor reactor, ILogger<ReactorWorker> logger)
    {
        _reactor = reactor;
        _logger = logger;
    }

    private Task _asyncReactor;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownTokenSource.Token);

        _asyncReactor = Task.Run(() => _reactor.Start(linkedTokenSource.Token), linkedTokenSource.Token);

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
            _logger.LogInformation("Reactor task was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while stopping the reactor.");
        }
    }

}
