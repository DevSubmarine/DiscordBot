using DevSubmarine.DiscordBot.Voting;
using DevSubmarine.DiscordBot.Voting.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class VotingDependencyInjectionExtensions
    {
        public static IServiceCollection AddVoting(this IServiceCollection services, Action<VotingOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.AddMongoDB();
            services.TryAddSingleton<IVotesStore, MongoVotesStore>();
            services.TryAddSingleton<IVotingCooldownManager, VotingCooldownManager>();
            services.TryAddTransient<IVotingService, VotingService>();

            return services;
        }
    }
}
