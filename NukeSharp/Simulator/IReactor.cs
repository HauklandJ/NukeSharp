namespace NukeSharp.Simulator;

public interface IReactor
{
    Task Start(CancellationToken cancellationToken);
}
