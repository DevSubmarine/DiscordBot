global using System;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Linq;
global using System.Text.RegularExpressions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DevSubmarine.DiscordBot.Client;
using Microsoft.Extensions.DependencyInjection;

namespace DevSubmarine.DiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsecrets.json", optional: true);
                    config.AddJsonFile($"appsecrets.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    // options
                    services.Configure<DiscordOptions>(context.Configuration);

                    // client services
                    services.AddDiscordClient();

                    // utilities

                    // features
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
