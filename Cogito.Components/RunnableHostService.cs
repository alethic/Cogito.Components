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
        /// Starts the <see cref="RunnableHost"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (run != null)
                throw new InvalidOperationException("RunnableHostService is already started.");

            cts = new CancellationTokenSource();
            run = host.RunAsync(CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token).Token);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the <see cref="RunnableHost"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (run == null)
                throw new InvalidOperationException("RunnableHostService is already stopped.");

            // signal service shutdown, wait for termination
            cts.Cancel();
            await run;
            run = null;
        }

    }

}
