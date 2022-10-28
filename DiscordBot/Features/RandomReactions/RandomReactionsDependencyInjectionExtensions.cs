using DevSubmarine.DiscordBot.RandomReactions;
using DevSubmarine.DiscordBot.RandomReactions.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RandomReactionsDependencyInjectionExtensions
    {
        public static IServiceCollection AddRandomReactions(this IServiceCollection services, Action<RandomReactionsOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.TryAddSingleton<IRandomReactionEmoteProvider, SortedRandomReactionEmoteProvider>();
            services.TryAddSingleton<IWelcomeTriggerProvider, WelcomeTriggerProvider>();
            services.AddHostedService<RandomReactionsListener>();

            return services;
        }
    }
}
