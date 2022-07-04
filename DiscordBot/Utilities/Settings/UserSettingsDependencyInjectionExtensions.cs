using DevSubmarine.DiscordBot.Settings;
using DevSubmarine.DiscordBot.Settings.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserSettingsDependencyInjectionExtensions
    {
        public static IServiceCollection AddUserSettings(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddMongoDB();
            services.TryAddSingleton<IUserSettingsStore, MongoUserSettingsStore>();
            services.TryAddTransient<IUserSettingsProvider, UserSettingsProvider>();

            return services;
        }
    }
}
