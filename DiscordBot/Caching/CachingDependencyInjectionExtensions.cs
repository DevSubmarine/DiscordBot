using DevSubmarine.DiscordBot.Caching;
using DevSubmarine.DiscordBot.Caching.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CachingDependencyInjectionExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services, Action<MemoryCacheOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMemoryCache(configureOptions);
            services.TryAddSingleton<ICacheProvider, CacheProvider>();

            return services;
        }
    }
}
