﻿using Discord;
using Discord.WebSocket;

namespace DevSubmarine.DiscordBot
{
    public static class DIscordUserExtensions
    {
        public static Task ModifyAsync(this IGuildUser user, Action<GuildUserProperties> func, CancellationToken cancellationToken)
            => user.ModifyAsync(func, new RequestOptions { CancelToken = cancellationToken });
        public static string GetMaxAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto)
            => GetSafeAvatarUrl(user, format, (ushort)(user is SocketUser ? 2048 : 1024));
        public static string GetSafeAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

        public static string GetUsernameWithDiscriminator(this IUser user)
            => $"{user.Username}#{user.Discriminator}";
    }
}
