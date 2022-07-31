using DevSubmarine.DiscordBot.RandomStatus;
using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using DevSubmarine.DiscordBot.RandomStatus.Services;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomStatusDependencyInjectionExtensions
    {
        public static IServiceCollection AddRandomStatus(this IServiceCollection services, Action<RandomStatusOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.AddSingleton<IStatusPlaceholderEngine, StatusPlaceholderEngine>(services =>
            {
                StatusPlaceholderEngine engine = ActivatorUtilities.CreateInstance<StatusPlaceholderEngine>(services);
                ILogger log = services.GetRequiredService<ILogger<StatusPlaceholderEngine>>();

                log.LogDebug("Loading all status placeholders from current assembly");
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().DefinedTypes.Where(t =>
                        typeof(IStatusPlaceholder).IsAssignableFrom(t) &&
                        !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) &&
                        Attribute.IsDefined(t, typeof(StatusPlaceholderAttribute), true));
                int count = engine.AddPlaceholders(types);
                log.LogInformation("Loaded {Count} status placeholders", count);
                return engine;
            });
            services.AddHostedService<RandomStatusService>();

            return services;
        }
    }
}
