using Discord;
using Discord.Interactions;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.Birthdays.Commands
{
    [Group("birthday", "Access user birthdays")]
    public class UserBirthdaysCommands : DevSubInteractionModule
    {
        private readonly IUserBirthdaysProvider _provider;
        private readonly IRandomizer _randomizer;

        private static readonly string[] _emotes = { ResponseEmoji.EyesBlurry, ResponseEmoji.Parrot60fps, ResponseEmoji.ParrotParty, ResponseEmoji.Zoop, ResponseEmoji.BlobHearts, ResponseEmoji.BlobHug };

        public UserBirthdaysCommands(IUserBirthdaysProvider provider, IRandomizer randomizer)
        {
            this._provider = provider;
            this._randomizer = randomizer;
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

            Embed embed = await this.GetUserBirthdayEmbedAsync(birthday).ConfigureAwait(false);
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
            IEnumerable<UserBirthday> todayBirthdays = GetTodayBirthdays(allBirthdays);
            IEnumerable<UserBirthday> upcomingBirthdays = GetUpcomingBirthdays(allBirthdays);

            if (!todayBirthdays.Any() && !upcomingBirthdays.Any())
            {
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"No upcoming birthdays found! {ResponseEmoji.FeelsDumbMan}",
                    base.GetRequestOptions());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithColor(this._randomizer.GetRandomDiscordColor());
            if (todayBirthdays.Any())
            {
                List<string> entries = new List<string>(todayBirthdays.Count());
                foreach (UserBirthday bday in todayBirthdays)
                {
                    IUser user = await base.Context.Client.GetUserAsync(bday.UserID, base.Context.CancellationToken).ConfigureAwait(false);
                    entries.Add($"Happy birthday {user.Mention} (`{user.GetUsernameWithDiscriminator()}`)! {this._randomizer.GetRandomValue(_emotes)}");
                }
                embed.AddField("Today's Birthdays!", string.Join('\n', entries));
            }
            if (upcomingBirthdays.Any())
            {
                List<string> entries = new List<string>(todayBirthdays.Count());
                foreach (UserBirthday bday in upcomingBirthdays)
                {
                    IUser user = await base.Context.Client.GetUserAsync(bday.UserID, base.Context.CancellationToken).ConfigureAwait(false);
                    DateTime date = (DateTime)bday.Date;
                    entries.Add($"{user.Mention} (`{user.GetUsernameWithDiscriminator()}`) - {TimestampTag.FromDateTime(date, TimestampTagStyles.LongDate)} ({TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)})");
                }
                embed.AddField("Upcoming Birthdays", string.Join('\n', entries));
            }

            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Embed = embed.Build();
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

            Embed embed = await this.GetUserBirthdayEmbedAsync(birthday).ConfigureAwait(false); ;
            await base.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = $"Birthday for user {user.Mention} (`{user.GetUsernameWithDiscriminator()}`) saved. {ResponseEmoji.Success}";
                msg.Embed = embed;
                msg.AllowedMentions = AllowedMentions.None;
            },
                base.GetRequestOptions());
        }

        private async Task<Embed> GetUserBirthdayEmbedAsync(UserBirthday birthday)
        {
            DateTime date = (DateTime)birthday.Date;
            IGuildUser guildUser = await base.Context.Guild.GetGuildUserAsync(birthday.UserID, base.Context.CancellationToken).ConfigureAwait(false);
            IUser user = guildUser ?? await base.Context.Client.GetUserAsync(birthday.UserID, base.Context.CancellationToken).ConfigureAwait(false);
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(guildUser != null ? guildUser.GetUserColour() : Color.Default)
                .WithTitle($"{user.GetName()}'s Birthday")
                .WithThumbnailUrl(user.GetMaxAvatarUrl())
                .WithAuthor(user);
            if (birthday.Date.IsToday)
                embed.WithDescription($"Today! It's today! Happy birthday! {this._randomizer.GetRandomValue(_emotes)}");
            else
                embed.WithDescription($"{TimestampTag.FromDateTime(date, TimestampTagStyles.LongDate)} ({TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)})")
                    .WithTimestamp(new DateTimeOffset(date));
           return embed.Build();
        }

        private static IEnumerable<UserBirthday> GetTodayBirthdays(IEnumerable<UserBirthday> birthdays)
            => birthdays.Where(birthday => birthday.Date.IsToday);

        private static IEnumerable<UserBirthday> GetUpcomingBirthdays(IEnumerable<UserBirthday> birthdays, int days = 7)
        {
            DateTime startDate = (DateTime)BirthdayDate.Today.AddDays(1);
            DateTime endDate = startDate.AddDays(days);
            return birthdays.Where(birthday
                => !birthday.Date.IsToday
                && (DateTime)birthday.Date >= startDate
                && (DateTime)birthday.Date <= endDate)
                .OrderBy(birthday => (DateTime)birthday.Date);
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
