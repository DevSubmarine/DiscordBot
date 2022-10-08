using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace DevSubmarine.DiscordBot.Tools
{
    public static class Logging
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static ILogger ConfigureLogging(IConfiguration configuration)
        {
            lock (_lock)
            {
                if (_initialized)
                    return Log.Logger;

                LoggerConfiguration config = new LoggerConfiguration();

                if (configuration?.GetSection("Serilog").Exists() == true)
                    config.ReadFrom.Configuration(configuration, "Serilog");
                else
                {
                    config.WriteTo.Console()
                        .WriteTo.File("logs/log.txt", fileSizeLimitBytes: 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                        .MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Verbose : LogEventLevel.Debug)
                            .Enrich.FromLogContext();
                }

                Log.Logger = config.CreateLogger();
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                _initialized = true;
                return Log.Logger;
            }
        }

        public static IServiceCollection AddToolsLogging(this IServiceCollection services, IConfiguration configuration = null)
        {
            ILogger log = ConfigureLogging(configuration);
            services.TryAddSingleton<ILoggerFactory>(new LoggerFactory()
                .AddSerilog(log, dispose: true));
            services.AddLogging();

            return services;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Fatal((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
