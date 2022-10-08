using Microsoft.Extensions.DependencyInjection.Extensions;
using DevSubmarine.DiscordBot.Time;
using DevSubmarine.DiscordBot.Time.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TimezonesDependencyInjectionExtensions
    {
        public static IServiceCollection AddTimezones(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ITimezoneProvider, TimezoneProvider>();
            services.TryAddSingleton<IValidateOptions<TimezoneOptions>, TimezoneOptionsValidator>();

            return services;
        }
    }
}
