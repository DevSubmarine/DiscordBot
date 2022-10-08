using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace DevSubmarine.DiscordBot.Tools
{
    public static class ToolLifetime
    {
        public static IConfiguration LoadConfiguration(string[] args)
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                //.AddJsonFile("appsettings.json", optional: true)
                //.AddJsonFile("appsecrets.json", optional: true)
                .AddCommandLine(args)
                .Build();
        }

        public static void Finalize(IServiceProvider services = null)
        {
            FlushLog();
            DisposeServices(services);
        }

        private static void FlushLog()
        {
            try { Log.CloseAndFlush(); } catch { }
        }

        private static void DisposeServices(IServiceProvider services)
        {
            if (services == null)
                return;

            try
            {
                if (services is IDisposable disposableServices)
                    disposableServices.Dispose();
            }
            catch { }
        }
    }
}
