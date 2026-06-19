using Microsoft.Extensions.Logging;
using NukeSharp.Services;

namespace NukeSharp.ControlSystem;

public class ReactorSystem
{
    private readonly IValveControl _valveControl;
    private readonly IPressureSensor _pressureSensor;
    private readonly ILogger<ReactorSystem> _logger;

    private volatile float _maxPressure = 0.71f;
    private volatile float _minPressure = 0.57f;
    private volatile bool _automationEnabled = true;

    public float MaxPressure => _maxPressure;
    public float MinPressure => _minPressure;
    public bool AutomationEnabled => _automationEnabled;

    public void SetThresholds(float openAt, float closeAt)
    {
        _maxPressure = openAt;
        _minPressure = closeAt;
    }

    public void TriggerEmergencyOpen()
    {
        _automationEnabled = false;
        _valveControl.Open();
    }

    public void RestartAutomation()
    {
        _automationEnabled = true;
    }

    public ReactorSystem(
        IValveControl valveControl,
        IPressureSensor pressureSensor,
        ILogger<ReactorSystem> logger
    )
    {
        _valveControl = valveControl;
        _pressureSensor = pressureSensor;
        _logger = logger;

        _pressureSensor.PressureChanged += HandlePressureChange;
    }

    private void HandlePressureChange(float newPressure)
    {
        _logger.LogInformation("SYSTEM current value: {newPressure}", newPressure);
        if (_automationEnabled is false)
        {
            return;
        }

        if (newPressure > MaxPressure && _valveControl.IsOpen() is false)
        {
            _valveControl.Open();
        }
        else if (newPressure < MinPressure && _valveControl.IsOpen())
        {
            _valveControl.Close();
        }
    }
}
