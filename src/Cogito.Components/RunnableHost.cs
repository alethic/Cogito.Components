using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Autofac.Core;

using Cogito.Autofac;


using Microsoft.Extensions.Logging;

namespace Cogito.Components
{

    /// <summary>
    /// Runs multiple <see cref="IRunnable"/> implementations.
    /// </summary>
    [RegisterAs(typeof(RunnableHost))]
    [RegisterInstancePerLifetimeScope]
    public class RunnableHost
    {

        public static readonly object RunnableScope = new object();

        readonly ILifetimeScope parent;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="logger"></param>
        public RunnableHost(ILifetimeScope parent, ILogger logger)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Raised when an exception occurs in an <see cref="IRunnable"/>.
        /// </summary>
        public event UnhandledExceptionEventHandler UnhandledException;

        /// <summary>
        /// Starts the services. 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Runnable host starting.");

            // run until cancelled
            while (cancellationToken.IsCancellationRequested == false)
            {
                var runnable = parent.ComponentRegistry.ServiceRegistrationsFor(new TypedService(typeof(IRunnable))).ToArray();
                if (runnable.Length > 0)
                    await Task.WhenAll(runnable.Select(i => Task.Run(() => RunAsync(i, cancellationToken))));

                // wait until cancelled
                if (cancellationToken.IsCancellationRequested == false)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore
                    }
                }
            }

            logger.LogInformation("Runnable host stopped.");
        }

        /// <summary>
        /// Keeps a <see cref="IRunnable"/> running.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task RunAsync(ServiceRegistration registration, CancellationToken cancellationToken)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            while (cancellationToken.IsCancellationRequested == false)
            {
                // each invocation gets it's own instance and lifetime scope
                using (var scope = parent.BeginLifetimeScope(RunnableScope))
                {
                    // instantiate runnable
                    var runnable = (IRunnable)scope.ResolveComponent(new ResolveRequest(new TypedService(typeof(IRunnable)), registration, Enumerable.Empty<Parameter>()));

                    try
                    {
                        logger.LogInformation("{Runnable} starting.", runnable.GetType());

                        // enter into runnable
                        await runnable.RunAsync(cancellationToken);

                        // delay for a second to prevent storms
                        if (cancellationToken.IsCancellationRequested == false)
                        {
                            try
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                            }
                            catch (OperationCanceledException)
                            {
                                // ignore
                            }
                        }

                        logger.LogInformation("{Runnable} stopped.", runnable.GetType());
                    }
                    catch (OperationCanceledException)
                    {
                        // ignore
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "{Runnable} threw an unexpected exception. Restarting...", runnable.GetType());

                        // signal any interested parties about the exception
                        UnhandledException?.Invoke(e, new UnhandledExceptionEventArgs(e, false));

                        // delay to prevent storms
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }
        }

    }

}
