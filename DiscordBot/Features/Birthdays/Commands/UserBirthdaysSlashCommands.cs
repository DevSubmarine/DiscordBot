using DevSubmarine.DiscordBot.Time;
using Discord;
using Discord.Interactions;

namespace DevSubmarine.DiscordBot.Birthdays.Commands
{
    [Group("birthday", "Access user birthdays")]
    public class UserBirthdaysSlashCommands : DevSubInteractionModule
    {
        private readonly IUserBirthdaysProvider _provider;
        private readonly IUserBirthdayEmbedBuilder _embedBuilder;
        private readonly ITimezoneProvider _timezones;

        public UserBirthdaysSlashCommands(IUserBirthdaysProvider provider, IUserBirthdayEmbedBuilder embedBuilder, ITimezoneProvider timezones)
        {
            this._provider = provider;
            this._embedBuilder = embedBuilder;
            this._timezones = timezones;
        }

        [SlashCommand("get", "Gets birthday for user")]
        public async Task CmdGetAsync(
            [Summary("User", "User to get birthday date of")] IUser user)
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

        [SlashCommand("upcoming", "Gets today's and upcoming birthdays")]
        public async Task CmdGetUpcomingAsync(
            [Summary("Days", "Amount of days to check bdays for. 1-366.")] int daysAhead = 7)
        {
            const int minDays = 1;
            const int maxDays = 366;
            if (daysAhead < minDays || daysAhead > maxDays)
            {
                await base.RespondAsync($"{ResponseEmoji.IAmAgony} Invalid days count, bruh. {minDays} to {maxDays} pls. {ResponseEmoji.FeelsDumbMan}",
                    ephemeral: true,
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);

            IEnumerable<UserBirthday> allBirthdays = await this._provider.GetAllAsync(base.CancellationToken).ConfigureAwait(false);
            Embed embed = await this._embedBuilder.BuildUpcomingBirthdaysEmbedAsync(allBirthdays, daysAhead, useEmotes: true, base.CancellationToken).ConfigureAwait(false);

            if (embed == null)
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"No upcoming birthdays in next {daysAhead} day(s) found! {ResponseEmoji.FeelsDumbMan}",
                    base.GetRequestOptions()).ConfigureAwait(false);
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
            [Summary("Month", "Month of birth"), Autocomplete(typeof(MonthAutocompleteHandler))] int month,
            [Summary("Year", "Year of birth"), MinValue(0)] int? year = null,
            [Summary("Timezone", "Your timezone"), Autocomplete(typeof(UserBirthdayTimezoneAutocompleteHandler))] string timezoneID = null)
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

            if (!BirthdayDate.Validate(day, month))
            {
                await base.RespondAsync($"{day}.{month} is not a valid date. {ResponseEmoji.Failure}",
                    ephemeral: true,
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(timezoneID))
                timezoneID = null;
            else if (!this._timezones.ContainsTimezone(timezoneID))
            {
                await base.RespondAsync($"What... the hell... is this timezone? {ResponseEmoji.FeelsBeanMan}",
                    ephemeral: false,
                    allowedMentions: AllowedMentions.None,
                    options: base.GetRequestOptions()).ConfigureAwait(false);
                return;
            }

            await base.DeferAsync(false, base.GetRequestOptions()).ConfigureAwait(false);
            BirthdayDate date = new BirthdayDate(day, month, year, timezoneID);
            UserBirthday birthday = new UserBirthday(user.Id, date);
            await this._provider.AddAsync(birthday, base.CancellationToken).ConfigureAwait(false);

            Embed embed = await this._embedBuilder.BuildUserBirthdayEmbedAsync(birthday, base.Context.Guild?.Id, base.CancellationToken).ConfigureAwait(false);
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
