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
            [Summary("Month", "Month of birth")] Month month,
            [Summary("Year", "Year of birth"), MinValue(0)] int? year = null)
        {
            if (year != null)
            {
                // for 31 dec allow year + 1 cause of timezones
                // this is primitive way to handle it, but really, who cares
                DateTime now = DateTime.UtcNow;
                int maxYear = now.Year;
                if (now.Day == 31 && now.Month == 12)
                    maxYear++;
                if (year > maxYear)
                {
                    await base.RespondAsync($"How tf would {user.Mention} be born in {year}? {ResponseEmoji.FeelsBeanMan} {ResponseEmoji.FeelsBeanMan} {ResponseEmoji.FeelsDumbMan}",
                        ephemeral: true,
                        allowedMentions: AllowedMentions.None,
                        options: base.GetRequestOptions()).ConfigureAwait(false);
                    return;
                }
            }

            if (!BirthdayDate.Validate(day, (int)month))
            {
                await base.RespondAsync($"{day}.{month} is not a valid date. {ResponseEmoji.Failure}",
                    ephemeral: true,
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);
            BirthdayDate date = new BirthdayDate(day, (int)month, year);
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
    }
}
