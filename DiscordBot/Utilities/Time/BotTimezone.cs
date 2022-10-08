using System.Diagnostics;

namespace DevSubmarine.DiscordBot.Time
{
    [DebuggerDisplay("{Name,nq}")]
    public class BotTimezone : IEquatable<BotTimezone>, IEquatable<TimeZoneInfo>
    {
        public TimeZoneInfo Timezone { get; }

        public string ID => this.Timezone.Id;
        public string Name => this.Timezone.DisplayName;
        public DateTime Now => TimeZoneInfo.ConvertTimeToUtc(DateTime.UtcNow, this.Timezone);

        public BotTimezone(TimeZoneInfo timezone)
        {
            this.Timezone = timezone;
        }

        public static BotTimezone Deserialize(string serializedTimezoneInfo)
        {
            TimeZoneInfo timezone = TimeZoneInfo.FromSerializedString(serializedTimezoneInfo);
            return new BotTimezone(timezone);
        }

        public override string ToString()
            => this.Name;

        public override bool Equals(object obj)
        {
            if (obj is BotTimezone btz)
                return this.Equals(btz);
            if (obj is TimeZoneInfo tzi)
                return this.Equals(tzi);
            return false;
        }

        public bool Equals(BotTimezone other)
            => this.Equals(other?.Timezone);

        public bool Equals(TimeZoneInfo other)
            => other is not null && this.Timezone.Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(this.Timezone);
    }
}
