# Cogito.Components

A small host for long-running work, built on Autofac assembly-module scanning.

## Why

Most applications grow a handful of things that need to start when the process starts and keep
running until it stops — a poller, a queue consumer, a scheduler. Wiring each one by hand into the
host means every new worker touches startup. Here a worker implements `IRunnable`, registers itself
like any other component, and the host finds it.

## Install

```shell
dotnet add package Cogito.Components
```

## Writing a worker

```csharp
[RegisterAs(typeof(IRunnable))]
public class ImportWorker : IRunnable
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken.IsCancellationRequested == false)
        {
            await ImportAsync(cancellationToken);
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }
    }
}
```

`RunAsync` is expected to keep running. Return when the cancellation token is signalled.

## Running them

`RunnableHost` resolves every `IRunnable` into its own lifetime scope and runs them together,
surfacing failures through an `UnhandledException` event:

```csharp
var host = new RunnableHost(container, logger);
host.UnhandledException += (s, e) => logger.Error((Exception)e.ExceptionObject, "worker failed");
await host.RunAsync(cancellationToken);
```

`RunnableHostService` wraps the same thing as an `IHostedService` for use with the generic host.

`ISynchronizationService` is the seam for coordinating workers that must not run concurrently across
instances.

## License

MIT.
