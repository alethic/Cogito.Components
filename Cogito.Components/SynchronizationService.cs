using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cogito.Components
{

    /// <summary>
    /// Provides a service by which a component can acquire a pessimistic lock.
    /// </summary>
    public interface ISynchronizationService
    {

        /// <summary>
        /// Acquires a lock with the specified name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> GetLockAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Acquires a lock with the specified name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> GetLockAsync(string key, TimeSpan timeout, CancellationToken cancellationToken = default);

    }

}
