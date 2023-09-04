namespace NukeSharp.Services;

public interface IPressureSensor
{
    event Action<float> PressureChanged;

    float GetValue();
    void Update(bool isValveOpen);
}
