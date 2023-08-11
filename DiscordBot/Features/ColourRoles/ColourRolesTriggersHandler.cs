using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.DiscordBot.ColourRoles
{
    internal class ColourRolesTriggersHandler : IHostedService, IDisposable
    {
        private readonly IColourRoleProvider _roleProvider;
        private readonly IColourRoleChanger _roleChanger;
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<DevSubOptions> _devsubOptions;

        private CancellationTokenSource _cts;

        public ColourRolesTriggersHandler(IColourRoleChanger roleChanger, IColourRoleProvider roleProvider, DiscordSocketClient client, IOptionsMonitor<DevSubOptions> devsubOptions)
        {
            this._roleChanger = roleChanger;
            this._roleProvider = roleProvider;
            this._client = client;
            this._devsubOptions = devsubOptions;

            this._client.MessageReceived += this.OnClientMessageReceived;
        }

        private async Task OnClientMessageReceived(SocketMessage message)
        {
            if (message.Channel is not SocketTextChannel channel)
                return;
            if (channel.Guild.Id != this._devsubOptions.CurrentValue.GuildID)
                return;

            if (!message.Content.StartsWith("colour me daddy", StringComparison.OrdinalIgnoreCase)
                && !message.Content.StartsWith("color me daddy", StringComparison.OrdinalIgnoreCase))
                return;

            CancellationToken cancellationToken = this._cts.Token;
            IGuildUser user = await channel.Guild.GetGuildUserAsync(message.Author.Id, cancellationToken).ConfigureAwait(false);
            IRole selectedRole = this._roleProvider.GetNewRandomRole(user);

            if (await this.SetUserRoleAsync(user, selectedRole, channel, cancellationToken).ConfigureAwait(false))
                await this.ConfirmRoleChangeAsync(user, selectedRole, channel, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> SetUserRoleAsync(IGuildUser user, IRole role, SocketTextChannel responseChannel, CancellationToken cancellationToken)
        {
            try
            {
                await this._roleChanger.SetUserRoleAsync(user, role);
                return true;
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                await responseChannel.SendMessageAsync($"Oops! {ResponseEmoji.Failure}\nI lack permissions to change your role! {ResponseEmoji.FeelsBeanMan}", 
                    options: cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                return false;
            }
        }

        private Task ConfirmRoleChangeAsync(IGuildUser user, IRole role, SocketTextChannel responseChannel, CancellationToken cancellationToken)
        {
            Embed embed = new EmbedBuilder()
                    .WithTitle("Colour Role changed!")
                    .WithAuthor(user)
                    .WithColor(role.Color)
                    .WithDescription($"You're now {role.Mention} {ResponseEmoji.EyesBlurry}")
                    .Build();
            return responseChannel.SendMessageAsync(
                embed: embed,
                allowedMentions: new AllowedMentions(AllowedMentionTypes.Users),
                options: cancellationToken.ToRequestOptions());
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try { this._cts?.Cancel(); } catch { }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._client.MessageReceived -= this.OnClientMessageReceived; } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
