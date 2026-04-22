using System.Threading.Tasks;

namespace NukeSharp.Services;

public class ValveControl : IValveControl
{
    private bool _isOpen = false;
    private bool _isLocked = false;
    private readonly object _lockObject = new();

    public void Open()
    {
        lock (_lockObject)
        {
            if (_isLocked)
                return;

            _isLocked = true;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000);
                lock (_lockObject)
                {
                    _isOpen = true;
                    _isLocked = false;
                }
            }
            catch
            {
                lock (_lockObject) { _isLocked = false; }
            }
        });
    }

    public void Close()
    {
        lock (_lockObject)
        {
            if (_isLocked)
                return;

            _isLocked = true;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000);
                lock (_lockObject)
                {
                    _isOpen = false;
                    _isLocked = false;
                }
            }
            catch
            {
                lock (_lockObject) { _isLocked = false; }
            }
        });
    }

    public bool IsOpen()
    {
        lock (_lockObject)
        {
            return _isOpen;
        }
    }
}
