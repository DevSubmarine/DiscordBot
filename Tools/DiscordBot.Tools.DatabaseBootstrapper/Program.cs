global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using DevSubmarine.DiscordBot.Database;
global using MongoDB.Driver;

using System.Diagnostics;
using DevSubmarine.DiscordBot.Database.Services;
using DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper.CollectionCreators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace DevSubmarine.DiscordBot.Tools.DatabaseBootstrapper
{
    class Program
    {
        private static IServiceProvider _services;

        static async Task Main(string[] args)
        {
            // config
            IConfiguration config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                //.AddJsonFile("appsettings.json", optional: true)
                //.AddJsonFile("appsecrets.json", optional: true)
                .AddCommandLine(args)
                .Build();

            // logging
            Logging.ConfigureLogging(config);

            try
            {
                // prepare DI container
                IServiceCollection serviceCollection = ConfigureServices(config);
                _services = serviceCollection.BuildServiceProvider();

                // run
                ApplicationRunner runner = _services.GetRequiredService<ApplicationRunner>();
                await runner.RunAsync().ConfigureAwait(false);

                // wait for enter on done
                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
            finally
            {
                OnExit();
            }
        }

        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();

            services.Configure<MongoOptions>(configuration);

            services.TryAddSingleton<ILoggerFactory>(new LoggerFactory()
                .AddSerilog(Log.Logger, dispose: true));
            services.AddLogging();
            services.AddTransient<ApplicationRunner>();
            services.AddSingleton<IMongoDatabaseClient, MongoDatabaseClient>();

            // COLLECTION CREATORS
            services.AddCollectionCreator<SubWordsCollectionCreator>();
            services.AddCollectionCreator<VotesCollectionCreator>();
            services.AddCollectionCreator<UserSettingsCollectionCreator>();

            return services;
        }

        private static void OnExit()
        {
            try { Log.CloseAndFlush(); } catch { }
            try
            {
                if (_services is IDisposable disposableServices)
                    disposableServices.Dispose();
            }
            catch { }
        }
    }
}
