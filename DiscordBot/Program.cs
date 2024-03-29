﻿global using System;
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
using Microsoft.Extensions.DependencyInjection;
using DevSubmarine.DiscordBot.Serialization;
using Newtonsoft.Json.Converters;

namespace DevSubmarine.DiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>()
                {
                    new UnixTimestampConverter(),
                    new StringEnumConverter()
                }
            };

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("timezones.json", optional: false);
                    config.AddJsonFile("appsecrets.json", optional: true);
                    config.AddJsonFile($"appsecrets.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureSerilog()
                .ConfigureServices((context, services) =>
                {
                    // options
                    services.Configure<DevSubOptions>(context.Configuration);
                    services.Configure<Client.DiscordOptions>(context.Configuration);
                    services.Configure<Time.TimezoneOptions>(context.Configuration);
                    services.Configure<Database.MongoOptions>(context.Configuration.GetSection("Database"));
                    services.Configure<SubWords.SubWordsOptions>(context.Configuration.GetSection("SubWords"));
                    services.Configure<ColourRoles.ColourRolesOptions>(context.Configuration.GetSection("ColourRoles"));
                    services.Configure<BlogsManagement.BlogsManagementOptions>(context.Configuration.GetSection("BlogsManagement"));
                    services.Configure<Voting.VotingOptions>(context.Configuration.GetSection("Voting"));
                    services.Configure<RandomStatus.RandomStatusOptions>(context.Configuration.GetSection("RandomStatus"));
                    services.Configure<Birthdays.UserBirthdaysOptions>(context.Configuration.GetSection("Birthdays"));
                    services.Configure<RandomReactions.RandomReactionsOptions>(context.Configuration.GetSection("RandomReactions"));

                    services.AddSingleton<IValidateOptions<DevSubOptions>, DevSubOptionsValidator>();

                    // dependencies
                    services.AddDiscordClient();
                    services.AddRandomizer();
                    services.AddMongoDB();
                    services.AddPasteMyst();
                    services.AddCaching();
                    services.AddUserSettings();
                    services.AddTimezones();

                    // features
                    services.AddSubWords();
                    services.AddBlogsManagement();
                    services.AddVoting();
                    services.AddRandomStatus();
                    services.AddUserBirthdays();
                    services.AddRandomReactions();
                    services.AddColourRoles();
                })
                .Build();
            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
