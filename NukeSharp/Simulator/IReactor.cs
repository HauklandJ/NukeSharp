using System.Threading;
using System.Threading.Tasks;

namespace NukeSharp.Simulator;

public interface IReactor
{
    Task Start(CancellationToken cancellationToken);
}
