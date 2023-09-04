namespace NukeSharp.Services;

public class RandomizedPressureSensor : IPressureSensor
{
    public event Action<float> PressureChanged;
    private float _backingPressure = 0.3f;
    internal float Pressure
    {
        get
        {
            return _backingPressure;
        }
        set
        {
            if (value > 1.00f) { _backingPressure = 1.00f; }
            else if (value < 0.00f) { _backingPressure = 0.00f; }
            else { _backingPressure = value; }
        }
    }
    private Random _random = new();
    public float GetValue()
    {
        return (float)Math.Round(Pressure, 3);
    }
    public void Update(bool isValveOpen)
    {
        if (isValveOpen)
        {
            Pressure *= 0.94f;  // Reduserer trykket med 6%
        }
        else
        {
            if (_random.NextDouble() > 0.95)
            {
                Pressure *= 1.08f;  // USTABILITET!!!! 
            }
            Pressure *= 1.03f;
        }
        PressureChanged?.Invoke(Pressure);
    }
}
