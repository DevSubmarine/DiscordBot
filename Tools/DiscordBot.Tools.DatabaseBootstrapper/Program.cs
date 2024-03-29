﻿global using System;
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

            services.Configure<MongoOptions>(configuration);

            services.AddToolsLogging(configuration);
            services.AddTransient<ApplicationRunner>();
            services.AddSingleton<IMongoDatabaseClient, MongoDatabaseClient>();

            // COLLECTION CREATORS
            services.AddCollectionCreator<SubWordsCollectionCreator>();
            services.AddCollectionCreator<VotesCollectionCreator>();
            services.AddCollectionCreator<UserSettingsCollectionCreator>();
            services.AddCollectionCreator<UserBirthdaysCollectionCreator>();

            return services;
        }
    }
}
