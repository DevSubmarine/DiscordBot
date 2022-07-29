using Discord;
using System.Text;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot.Birthdays.Services
{
    internal class UserBirthdayEmbedBuilder : IUserBirthdayEmbedBuilder
    {
        private readonly IDiscordClient _client;
        private readonly IRandomizer _randomizer;

        private static readonly string[] _emotes = { ResponseEmoji.EyesBlurry, ResponseEmoji.Parrot60fps, ResponseEmoji.ParrotParty, ResponseEmoji.Zoop, ResponseEmoji.BlobHearts, ResponseEmoji.BlobHug };

        public UserBirthdayEmbedBuilder(IDiscordClient client, IRandomizer randomizer)
        {
            this._client = client;
            this._randomizer = randomizer;
        }

        public async Task<Embed> BuildUpcomingBirthdaysEmbedAsync(IEnumerable<UserBirthday> birthdays, bool useEmotes, CancellationToken cancellationToken = default)
        {
            IEnumerable<UserBirthday> todayBirthdays = GetTodayBirthdays(birthdays);
            IEnumerable<UserBirthday> upcomingBirthdays = GetUpcomingBirthdays(birthdays);

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
                        entryBuilder.Append($"{GetAge(birthday).GetOrdinalString()} ");
                    entryBuilder.Append($"birthday {user.Mention} (`{user.GetUsernameWithDiscriminator()}`)! {emote}");
                    entries.Add(entryBuilder.ToString());
                }
                embed.AddField("Today's Birthdays!", string.Join('\n', entries));
            }
            if (upcomingBirthdays.Any())
            {
                List<string> entries = new List<string>(todayBirthdays.Count());
                foreach (UserBirthday birthday in upcomingBirthdays)
                {
                    entryBuilder.Clear();
                    IUser user = await this._client.GetUserAsync(birthday.UserID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                    DateTime date = (DateTime)birthday.Date;
                    entryBuilder.Append($"{user.Mention} (`{user.GetUsernameWithDiscriminator()}`) - {TimestampTag.FromDateTime(date, TimestampTagStyles.LongDate)} ({TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)})");
                    if (birthday.Date.Year != null)
                        entryBuilder.Append($" (will be {(GetAge(birthday) + 1)})");
                    entries.Add(entryBuilder.ToString());
                }
                embed.AddField("Upcoming Birthdays", string.Join('\n', entries));
            }

            return embed.Build();
        }

        public async Task<Embed> BuildUserBirthdayEmbedAsync(UserBirthday birthday, ulong? guildID, CancellationToken cancellationToken = default)
        {
            DateTime date = (DateTime)birthday.Date;
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
            if (birthday.Date.IsToday)
            {
                if (birthday.Date.Year != null)
                {
                    int age = GetAge(birthday);
                    embed.WithDescription($"Today! It's today! They're {age} now! Happy birthday! {this._randomizer.GetRandomValue(_emotes)}");
                }
                else
                    embed.WithDescription($"Today! It's today! Happy birthday! {this._randomizer.GetRandomValue(_emotes)}");
            }
            else
                embed.WithDescription($"{TimestampTag.FromDateTime(date, TimestampTagStyles.LongDate)} ({TimestampTag.FromDateTime(date, TimestampTagStyles.Relative)})")
                    .WithTimestamp(new DateTimeOffset(date));

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
    }
}
