using System.Collections.Concurrent;

namespace NukeSharp;

public class PressureHistory
{
    private readonly ConcurrentQueue<float> _readings = new();

    public void Record(float pressure)
    {
        if (_readings.Count >= 100)
            _readings.TryDequeue(out _);
        _readings.Enqueue(pressure);
    }

    public float[] GetAll() => [.. _readings];
}
