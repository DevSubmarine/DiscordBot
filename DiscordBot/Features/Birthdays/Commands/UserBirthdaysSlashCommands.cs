using Discord;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot.Birthdays.Commands
{
    [Group("birthday", "Access user birthdays")]
    public class UserBirthdaysSlashCommands : DevSubInteractionModule
    {
        private readonly IUserBirthdaysProvider _provider;
        private readonly IUserBirthdayEmbedBuilder _embedBuilder;

        public UserBirthdaysSlashCommands(IUserBirthdaysProvider provider, IUserBirthdayEmbedBuilder embedBuilder)
        {
            this._provider = provider;
            this._embedBuilder = embedBuilder;
        }

        [SlashCommand("get", "Gets birthday for user")]
        public async Task CmdGetAsync(
            [Summary("User", "User to get birthday date of")] IUser user)
        {
            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);

            UserBirthday birthday = await this._provider.GetAsync(user.Id, base.Context.CancellationToken).ConfigureAwait(false);
            if (birthday == null)
            {
                await base.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = $"No birthday date found for user {user.Mention} (`{user.Mention}`). {ResponseEmoji.FeelsBeanMan}";
                    msg.AllowedMentions = AllowedMentions.None;
                },
                    base.GetRequestOptions());
                return;
            }

            Embed embed = await this._embedBuilder.BuildUserBirthdayEmbedAsync(birthday, base.Context.Guild?.Id, base.Context.CancellationToken).ConfigureAwait(false);
            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed;
                msg.AllowedMentions = AllowedMentions.None;
            },
                base.GetRequestOptions());
        }

        [SlashCommand("upcoming", "Gets today's and upcoming birthdays")]
        public async Task CmdGetUpcomingAsync()
        {
            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);

            IEnumerable<UserBirthday> allBirthdays = await this._provider.GetAllAsync(base.Context.CancellationToken).ConfigureAwait(false);
            Embed embed = await this._embedBuilder.BuildUpcomingBirthdaysEmbedAsync(allBirthdays, true, base.Context.CancellationToken).ConfigureAwait(false);

            if (embed == null)
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"No upcoming birthdays found! {ResponseEmoji.FeelsDumbMan}",
                    base.GetRequestOptions());
                return;
            }

            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed;
                msg.AllowedMentions = AllowedMentions.None;
            },
                base.GetRequestOptions());
        }

        [SlashCommand("update", "Saves user's birthday date")]
        public async Task CmdSetAsync(
            [Summary("User", "User to set birthday of")] IUser user,
            [Summary("Day", "Day of the month"), MinValue(1), MaxValue(31)] int day,
            [Summary("Month", "Month")] Month month)
        {
            if (!BirthdayDate.Validate(day, (int)month))
            {
                await base.RespondAsync($"{day} {month} is not a valid date. {ResponseEmoji.Failure}",
                    ephemeral: true,
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);
            BirthdayDate date = new BirthdayDate(day, (int)month);
            UserBirthday birthday = new UserBirthday(user.Id, date);
            await this._provider.AddAsync(birthday, base.Context.CancellationToken).ConfigureAwait(false);

            Embed embed = await this._embedBuilder.BuildUserBirthdayEmbedAsync(birthday, base.Context.Guild?.Id, base.Context.CancellationToken).ConfigureAwait(false);
            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = $"Birthday for user {user.Mention} (`{user.GetUsernameWithDiscriminator()}`) saved. {ResponseEmoji.Success}";
                msg.Embed = embed;
                msg.AllowedMentions = AllowedMentions.None;
            },
                base.GetRequestOptions());
        }

        public enum Month
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
        }
    }
}
