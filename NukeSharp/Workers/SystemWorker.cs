using NukeSharp.ControlSystem;

namespace NukeSharp.Workers;

internal class SystemWorker : IHostedService
{
    private readonly IReactorSystem _reactorSystem;
    private readonly CancellationTokenSource _shutdownTokenSource = new();
    private readonly ILogger<SystemWorker> _logger;

    public SystemWorker(IReactorSystem reactorSystem, ILogger<SystemWorker> logger)
    {
        _reactorSystem = reactorSystem;
        _logger = logger;
    }
    private Task _asyncReactorSystem;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdownTokenSource.Token);
        _asyncReactorSystem = Task.Run(
            () => _reactorSystem.Start(linkedTokenSource.Token), 
            linkedTokenSource.Token
        );
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdownTokenSource.Cancel();

        try
        {
            await _asyncReactorSystem;
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("System task was canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while stopping the reactor.");
        }
    }
}
