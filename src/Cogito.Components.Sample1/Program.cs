using System.Threading.Tasks;

using Cogito.Autofac;
using Cogito.Autofac.DependencyInjection;

using Microsoft.Extensions.Hosting;

namespace Cogito.Components.Sample1
{

    public static class Program
    {
        public static async Task Main(string[] args) => await Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory(b => b.RegisterAllAssemblyModules()))
            .RunConsoleAsync();

    }

}
