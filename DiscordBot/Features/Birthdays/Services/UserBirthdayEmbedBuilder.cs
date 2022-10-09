using DevSubmarine.DiscordBot.Time;
using Discord;
using System.Text;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.Birthdays.Services
{
    internal class UserBirthdayEmbedBuilder : IUserBirthdayEmbedBuilder
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;
        private readonly ITimezoneProvider _timezoneProvider;

        private static readonly string[] _emotes = { ResponseEmoji.EyesBlurry, ResponseEmoji.Parrot60fps, ResponseEmoji.ParrotParty, ResponseEmoji.Zoop, ResponseEmoji.BlobHearts, ResponseEmoji.BlobHug, ResponseEmoji.PepeComfy };

        public UserBirthdayEmbedBuilder(IDiscordClient client, IRandomizer randomizer, ITimezoneProvider timezoneProvider)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._timezoneProvider = timezoneProvider;
        }

        public async Task<Embed> BuildUpcomingBirthdaysEmbedAsync(IEnumerable<UserBirthday> birthdays, int daysAhead, bool useEmotes, CancellationToken cancellationToken = default)
        {
            IEnumerable<UserBirthday> todayBirthdays = this.GetTodayBirthdays(birthdays);
            IEnumerable<UserBirthday> upcomingBirthdays = this.GetUpcomingBirthdays(birthdays, daysAhead);

            if (!todayBirthdays.Any() && !upcomingBirthdays.Any())
                return null;

            EmbedBuilder embed = new EmbedBuilder()
                .WithCurrentTimestamp()
                .WithColor(this._randomizer.GetRandomDiscordColor());
            StringBuilder entryBuilder = new StringBuilder();
            if (todayBirthdays.Any())
            {
                List<string> entries = new List<string>(todayBirthdays.Count());
                foreach (UserBirthday birthday in todayBirthdays)
                {
                    entryBuilder.Clear();
                    string emote = useEmotes ? this._randomizer.GetRandomValue(_emotes) : null;
                    IUser user = await this._client.GetUserAsync(birthday.UserID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                    entryBuilder.Append("Happy ");
                    if (birthday.Date.Year != null)
                        entryBuilder.Append($"**{GetAge(birthday).GetOrdinalString()}** ");
                    entryBuilder.Append($"birthday {user.Mention} (`{user.GetUsernameWithDiscriminator()}`)! {emote}");
                    entries.Add(entryBuilder.ToString());
                }
                embed.AddField("Today's Birthdays!", string.Join('\n', entries));
            }
            if (upcomingBirthdays.Any())
            {
                const int maxItems = 10;
                List<string> entries = new List<string>(maxItems + 1);
                foreach (UserBirthday birthday in upcomingBirthdays.Take(maxItems))
                {
                    entryBuilder.Clear();
                    IUser user = await this._client.GetUserAsync(birthday.UserID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                    entryBuilder.Append($"{user.Mention} (`{user.GetUsernameWithDiscriminator()}`) - **{birthday.Date}** {this.GetTimestamp(birthday.Date)}");
                    if (birthday.Date.Year != null)
                        entryBuilder.Append($" (will be **{(GetAge(birthday) + 1)}**)");
                    entries.Add(entryBuilder.ToString());
                }
                if (upcomingBirthdays.Count() > maxItems)
                {
                    int remaining = upcomingBirthdays.Count() - maxItems;
                    entries.Add($"... and {remaining} more! {ResponseEmoji.Hype}");
                }
                embed.AddField($"Upcoming Birthdays in next {daysAhead} day(s)", string.Join('\n', entries));
            }

            if (todayBirthdays.Count() == 1)
            {
                UserBirthday birthday = todayBirthdays.Single();
                IUser user = await this._client.GetUserAsync(birthday.UserID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                embed.WithThumbnailUrl(user.GetMaxAvatarUrl());
            }

            return embed.Build();
        }

        public async Task<Embed> BuildUserBirthdayEmbedAsync(UserBirthday birthday, ulong? guildID, CancellationToken cancellationToken = default)
        {
            IGuildUser guildUser = null;
            if (guildID != null)
            {
                IGuild guild = await this._client.GetGuildAsync(guildID.Value, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (guild != null)
                    guildUser = await guild.GetGuildUserAsync(birthday.UserID, cancellationToken).ConfigureAwait(false);
            }
            IUser user = guildUser ?? await this._client.GetUserAsync(birthday.UserID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(guildUser != null ? guildUser.GetUserColour() : Color.Default)
                .WithTitle($"{user.GetName()}'s Birthday")
                .WithThumbnailUrl(user.GetMaxAvatarUrl())
                .WithAuthor(user);
            if (birthday.Date.Year != null)
                embed.AddField("Age", GetAge(birthday));
            if (birthday.Date.IsToday(this._timezoneProvider))
            {
                if (birthday.Date.Year != null)
                {
                    int age = GetAge(birthday);
                    embed.WithDescription($"Today! It's today! They're **{age}** now! Happy birthday! {this._randomizer.GetRandomValue(_emotes)}");
                }
                else
                    embed.WithDescription($"Today! It's today! Happy birthday! {this._randomizer.GetRandomValue(_emotes)}");
            }
            else
            {
                embed.WithDescription($"**{birthday.Date}** {this.GetTimestamp(birthday.Date)}");
            }

            return embed.Build();
        }

        private static int GetAge(UserBirthday birthday)
        {
            DateTime now = DateTime.UtcNow.Date;
            int age = now.Year - birthday.Date.Year.Value;
            if (birthday.Date.Month > now.Month || (birthday.Date.Month == now.Month && birthday.Date.Day > now.Day))
                age--;
            if (age < 0)
                age = 0;
            return age;
        }

        private IEnumerable<UserBirthday> GetTodayBirthdays(IEnumerable<UserBirthday> birthdays)
            => birthdays.Where(birthday => birthday.Date.IsToday(this._timezoneProvider));

        private IEnumerable<UserBirthday> GetUpcomingBirthdays(IEnumerable<UserBirthday> birthdays, int days = 7)
        {
            TimeSpan maxDiff = TimeSpan.FromDays(days);
            return birthdays.Where(birthday =>
            {
                TimeSpan diff = birthday.Date.GetTimeRemaining(this._timezoneProvider);
                return diff <= maxDiff && !birthday.Date.IsToday(this._timezoneProvider);
            })
                .OrderBy(birthday => (DateTime)birthday.Date);
        }

        private TimestampTag GetTimestamp(BirthdayDate date)
            => TimestampTag.FromDateTime(date.GetNextTimestamp(this._timezoneProvider), TimestampTagStyles.Relative);
    }
}
