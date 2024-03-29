﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using DevSubmarine.DiscordBot;
using DevSubmarine.DiscordBot.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordClient(this IServiceCollection services, Action<DiscordOptions> configureOptions = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configureOptions != null)
                services.Configure(configureOptions);

            services.TryAddSingleton<IValidateOptions<DiscordOptions>, DiscordOptionsValidator>();

            services.TryAddSingleton<IHostedDiscordClient, HostedDiscordClient>();
            services.AddTransient<IHostedService>(s => (IHostedService)s.GetRequiredService<IHostedDiscordClient>());
            services.TryAddSingleton<IDiscordClient>(s => s.GetRequiredService<IHostedDiscordClient>().Client);
            services.TryAddSingleton<DiscordSocketClient>(s => (DiscordSocketClient)s.GetRequiredService<IDiscordClient>());

            services.AddHostedService<DiscordCommandsService>();

            return services;
        }
    }
}
