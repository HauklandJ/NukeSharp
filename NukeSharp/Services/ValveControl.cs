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

        Task.Run(async () =>
        {
            await Task.Delay(2000); // 2 seconds
            _isOpen = true;

            lock (_lockObject)
            {
                _isLocked = false;
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

        Task.Run(async () =>
        {
            await Task.Delay(2000); // 2 seconds
            _isOpen = false;

            lock (_lockObject)
            {
                _isLocked = false;
            }
        });
    }

    public bool IsOpen()
    {
        return _isOpen;
    }
}
