using System.Threading;
using System.Threading.Tasks;

namespace NukeSharp.ControlSystem;

public interface IReactorSystem
{
    Task Start(CancellationToken cancellationToken);
}
