using DevSubmarine.DiscordBot.RandomStatus;
using DevSubmarine.DiscordBot.RandomStatus.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            services.AddSingleton<IStatusPlaceholderEngine, StatusPlaceholderEngine>();
            services.AddHostedService<RandomStatusService>();

            return services;
        }
    }
}
