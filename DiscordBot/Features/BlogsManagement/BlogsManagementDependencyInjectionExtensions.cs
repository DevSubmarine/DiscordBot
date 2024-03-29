﻿using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.BlogsManagement.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BlogsManagementDependencyInjectionExtensions
    {
        public static IServiceCollection AddBlogsManagement(this IServiceCollection services, Action<BlogsManagementOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.TryAddSingleton<IValidateOptions<BlogsManagementOptions>, BlogsManagementOptionsValidator>();
            services.TryAddTransient<IBlogChannelActivator, BlogChannelActivator>();
            services.TryAddTransient<IBlogChannelSorter, BlogChannelSorter>();
            services.TryAddTransient<IBlogChannelNameConverter, BlogChannelNameConverter>();
            services.TryAddTransient<IBlogChannelManager, BlogChannelManager>();
            services.AddHostedService<BlogActivityScanner>();
            services.AddHostedService<BlogActivityListener>();

            return services;
        }
    }
}
