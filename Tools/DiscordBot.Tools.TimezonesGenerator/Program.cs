global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace DevSubmarine.DiscordBot.Tools.TimezonesGenerator
{
    internal class Program
    {
        private static IServiceProvider _services;

        static async Task Main(string[] args)
        {
            IConfiguration config = ToolLifetime.LoadConfiguration(args);
            Logging.ConfigureLogging(config);

            try
            {
                IServiceCollection serviceCollection = ConfigureServices(config);
                _services = serviceCollection.BuildServiceProvider();

                ApplicationRunner runner = _services.GetRequiredService<ApplicationRunner>();
                await runner.RunAsync().ConfigureAwait(false);

                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
            finally
            {
                ToolLifetime.Finalize(_services);
            }
        }

        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddToolsLogging(configuration);
            services.AddTransient<ApplicationRunner>();

            return services;
        }
    }
}