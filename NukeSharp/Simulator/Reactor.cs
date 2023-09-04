using NukeSharp.Services;

namespace NukeSharp.Simulator;

public class Reactor : IReactor
{
    private readonly IValveControl _valve;

    private readonly IPressureSensor _sensor;
    private readonly ILogger<Reactor> _logger;

    public Reactor(IPressureSensor sensor, IValveControl valve, ILogger<Reactor> logger)
    {
        _sensor = sensor;
        _valve = valve;
        _logger = logger;
    }
    public async Task Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started reactor");
        while (!cancellationToken.IsCancellationRequested)
        {
            var isOpen = _valve.IsOpen();
            await Task.Delay(1000, cancellationToken);
            _sensor.Update(isOpen);
        }
    }
}