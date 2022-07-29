using DevSubmarine.DiscordBot.Birthdays;
using DevSubmarine.DiscordBot.Birthdays.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UserBirthdaysDependencyInjectionExtensions
    {
        public static IServiceCollection AddUserBirthdays(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddTransient<IUserBirthdaysStore, MongoUserBirthdayStore>();
            services.TryAddSingleton<IUserBirthdaysProvider, UserBirthdaysProvider>();

            return services;
        }
    }
}
