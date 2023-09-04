namespace NukeSharp.ControlSystem;

public interface IReactorSystem
{
    Task Start(CancellationToken cancellationToken);
}
