namespace NukeSharp.Services;

public interface IValveControl
{
    void Open();
    void Close();
    bool IsOpen();
}
