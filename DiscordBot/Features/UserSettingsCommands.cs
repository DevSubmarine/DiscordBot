using DevSubmarine.DiscordBot.Settings;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot.Features
{
    [Group("user-settings", "Allows you to edit your individual bot settings")]
    [EnabledInDm(true)]
    public class UserSettingsCommands : DevSubInteractionModule
    {
        private readonly IUserSettingsProvider _provider;

        public UserSettingsCommands(IUserSettingsProvider provider)
        {
            this._provider = provider;
        }

        [SlashCommand("vote-ping", "Allows you to change if you'll be pinged on a vote against you")]
        public async Task CmdVotePingAsync(
            [Summary("Enabled", "Whether the ping on vote will be enabled")] bool enabled)
        {
            await base.DeferAsync(options: base.GetRequestOptions()).ConfigureAwait(false);

            await this._provider.UpdateUserSettingsAsync(
                base.Context.User.Id,
                settings => settings.PingOnVote = enabled, 
                base.Context.CancellationToken).ConfigureAwait(false);

            string confirmationText = enabled
                ? $"{ResponseEmoji.Success} You'll now be pinged whenever someone casts a vote against you."
                : $"{ResponseEmoji.Success} You'll no longer be notified when someone votes against you.";
            await base.ModifyOriginalResponseAsync(msg => msg.Content = confirmationText, base.GetRequestOptions()).ConfigureAwait(false);
        }
    }
}
