using DevSubmarine.DiscordBot.SubWords;
using DevSubmarine.DiscordBot.SubWords.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SubWordsDependencyInjectionExtensions
    {
        public static IServiceCollection AddSubWords(this IServiceCollection services, Action<SubWordsOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.AddPasteMyst();
            services.AddMongoDB();
            services.TryAddSingleton<ISubWordsStore, MongoSubWordsStore>();
            services.TryAddTransient<ISubWordsService, SubWordsService>();

            return services;
        }
    }
}
