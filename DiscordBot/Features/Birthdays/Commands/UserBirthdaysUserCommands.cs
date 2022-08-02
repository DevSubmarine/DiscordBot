using Discord;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot.Birthdays.Commands
{
    public class UserBirthdaysUserCommands : DevSubInteractionModule
    {
        private readonly IUserBirthdaysProvider _provider;
        private readonly IUserBirthdayEmbedBuilder _embedBuilder;

        public UserBirthdaysUserCommands(IUserBirthdaysProvider provider, IUserBirthdayEmbedBuilder embedBuilder)
        {
            this._provider = provider;
            this._embedBuilder = embedBuilder;
        }

        [UserCommand("Check Birthday")]
        public async Task CmdGetAsync(IUser user)
        {
            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);

            UserBirthday birthday = await this._provider.GetAsync(user.Id, base.CancellationToken).ConfigureAwait(false);
            if (birthday == null)
            {
                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"No birthday date found for user {user.Mention} (`{user.GetUsernameWithDiscriminator()}`). {ResponseEmoji.FeelsBeanMan}";
                    msg.AllowedMentions = AllowedMentions.None;
                },
                    base.GetRequestOptions());
                return;
            }

            Embed embed = await this._embedBuilder.BuildUserBirthdayEmbedAsync(birthday, base.Context.Guild?.Id, base.CancellationToken).ConfigureAwait(false);
            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed;
                msg.AllowedMentions = AllowedMentions.None;
            },
                base.GetRequestOptions());
        }
    }
}
