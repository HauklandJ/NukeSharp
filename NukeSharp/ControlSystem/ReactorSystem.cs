using NukeSharp.Services;

namespace NukeSharp.ControlSystem;

public class ReactorSystem : IReactorSystem
{
    private readonly IValveControl _valveControl;
    private readonly IPressureSensor _pressureSensor;
    private readonly ILogger<ReactorSystem> _logger;

    private readonly float MAX_PRESSURE = 0.71f;
    private readonly float MIN_PRESSURE = 0.57f;

    public ReactorSystem(IValveControl valveControl, IPressureSensor pressureSensor, ILogger<ReactorSystem> logger)
    {
        _valveControl = valveControl;
        _pressureSensor = pressureSensor;
        _logger = logger;

        _pressureSensor.PressureChanged += HandlePressureChange;
    }

    private void HandlePressureChange(float newPressure)
    {
        _logger.LogInformation($"SYSTEM current value: {newPressure}");
        if (newPressure > MAX_PRESSURE && !_valveControl.IsOpen())
        {
            _valveControl.Open();
        }
        else if (newPressure < MIN_PRESSURE && _valveControl.IsOpen())
        {
            _valveControl.Close();
        }
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Started ReactorSystem");
        while (!cancellationToken.IsCancellationRequested) { }
    }

}
