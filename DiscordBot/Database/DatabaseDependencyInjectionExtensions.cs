﻿using DevSubmarine.DiscordBot.Database;
using DevSubmarine.DiscordBot.Database.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseDependencyInjectionExtensions
    {
        public static IServiceCollection AddMongoDB(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<IMongoDatabaseClient, MongoDatabaseClient>();

            return services;
        }
    }
}
