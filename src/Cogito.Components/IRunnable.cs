using System.Threading;
using System.Threading.Tasks;

namespace Cogito.Components
{

    /// <summary>
    /// Describes a service that should be run at startup and kept running.
    /// </summary>
    public interface IRunnable
    {

        /// <summary>
        /// Implement this method to run.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RunAsync(CancellationToken cancellationToken);

    }

}
