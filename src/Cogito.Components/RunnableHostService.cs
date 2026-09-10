using System;
using System.Threading;
using System.Threading.Tasks;

using Cogito.Autofac;

using Microsoft.Extensions.Hosting;

namespace Cogito.Components
{

    /// <summary>
    /// Exposes the <see cref="RunnableHost"/> to the Microsoft Hosting framework.
    /// </summary>
    [RegisterAs(typeof(IHostedService))]
    public class RunnableHostService : IHostedService
    {

        readonly RunnableHost host;
        readonly SemaphoreSlim sync = new SemaphoreSlim(1, 1);
        CancellationTokenSource cts;
        Task run;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="host"></param>
        public RunnableHostService(RunnableHost host)
        {
            this.host = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>
        /// Starts the <see cref="RunnableHost"/>. Returns without doing anything when it is already running.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await sync.WaitAsync(cancellationToken);

            try
            {
                if (run != null)
                    return;

                cts = new CancellationTokenSource();
                run = host.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token);
            }
            finally
            {
                sync.Release();
            }
        }

        /// <summary>
        /// Stops the <see cref="RunnableHost"/>. Returns without doing anything when it is not running; a caller that
        /// arrives while another is stopping it returns once that one has finished.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await sync.WaitAsync(cancellationToken);

            try
            {
                if (run == null)
                    return;

                // signal service shutdown, wait for termination
                cts.Cancel();

                try
                {
                    await run;
                }
                finally
                {
                    run = null;
                }
            }
            finally
            {
                sync.Release();
            }
        }

    }

}
