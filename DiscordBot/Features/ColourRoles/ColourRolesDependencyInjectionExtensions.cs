using DevSubmarine.DiscordBot.ColourRoles;
using DevSubmarine.DiscordBot.ColourRoles.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ColourRolesDependencyInjectionExtensions
    {
        public static IServiceCollection AddColourRoles(this IServiceCollection services, Action<ColourRolesOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.TryAddTransient<IColourRoleProvider, ColourRoleProvider>();
            services.TryAddTransient<IColourRoleChanger, ColourRoleChanger>();
            services.AddHostedService<ColourRolesTriggersHandler>();

            return services;
        }
    }
}
