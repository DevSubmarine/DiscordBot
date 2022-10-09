using DevSubmarine.DiscordBot.Time;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace DevSubmarine.DiscordBot.Birthdays
{
    // this should be struct but mongodb doesn't like deserializing them
    public class BirthdayDate : IEquatable<BirthdayDate>, IEquatable<DateTime>
    {
        [BsonElement("day")]
        public int Day { get; }
        [BsonElement("month")]
        public int Month { get; }
        [BsonElement("year"), BsonDefaultValue(null)]
        public int? Year { get; }
        [BsonElement, BsonIgnoreIfNull, BsonDefaultValue(null)]
        public string TimezoneID { get; }

        [BsonConstructor(nameof(Day), nameof(Month), nameof(Year), nameof(TimezoneID))]
        public BirthdayDate(int day, int month, int? year, string timezoneID)
        {
            if (!Validate(day, month))
                throw new ArgumentException($"{day}.{month} is not a valid date.");
            this.Day = day;
            this.Month = month;
            this.Year = year;

            if (!string.IsNullOrWhiteSpace(timezoneID))
                this.TimezoneID = timezoneID;
        }

        public BirthdayDate(DateTime date, string timezoneID)
            : this(date.Day, date.Month, date.Year, timezoneID) { }

        public BirthdayDate AddDays(int days)
        {
            DateTime dt = (DateTime)this;
            return new BirthdayDate(dt.AddDays(days), this.TimezoneID);
        }

        public bool IsToday(ITimezoneProvider timezoneProvider)
        {
            DateTime timestamp = (DateTime)this;
            DateTime localizedNow = DateTime.UtcNow;
            if (this.TimezoneID != null)
            {
                BotTimezone timezone = timezoneProvider.GetTimezone(this.TimezoneID);
                if (timezone == null)
                    throw new InvalidTimeZoneException($"Timezone with ID {timezone.ID} is invalid.");
                TimeSpan offset = timezone.Timezone.GetUtcOffset(localizedNow);
                localizedNow = localizedNow.Add(offset);
            }

            return localizedNow.Day == timestamp.Day && localizedNow.Month == timestamp.Month;
        }

        public TimeSpan GetTimeRemaining(ITimezoneProvider timezoneProvider)
        {
            DateTime value = this.GetNextTimestamp(timezoneProvider);
            return value - DateTime.UtcNow;
        }

        public override bool Equals(object obj)
        {
            if (obj is BirthdayDate bday)
                return this.Equals(bday);
            if (obj is DateTime dt)
                return this.Equals(dt);
            return false;
        }

        public bool Equals(BirthdayDate other)
            => this.Equals((DateTime)other);

        public bool Equals(DateTime other)
        {
            DateTime dt = (DateTime)this;
            return dt.Day == other.Day && dt.Month == other.Month;
        }

        public override int GetHashCode()
        {
            DateTime dt = (DateTime)this;
            return HashCode.Combine(dt.Day, dt.Month);
        }

        public static bool Validate(int day, int month)
        {
            // validate that the date is valid
            // using 2020 as it's a leap year, so validation of 29th feb won't fail
            try
            {
                _ = new DateTime(2020, month, day, 0, 0, 0, DateTimeKind.Unspecified);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public DateTime GetNextTimestamp(ITimezoneProvider timezoneProvider)
        {
            DateTime invariantDate = (DateTime)this;

            if (this.TimezoneID == null)
                return invariantDate;

            BotTimezone timezone = timezoneProvider.GetTimezone(this.TimezoneID);
            TimeSpan offset = timezone.Timezone.GetUtcOffset(DateTime.UtcNow);
            DateTime result = invariantDate.Subtract(offset);
            return new DateTime(result.Ticks, DateTimeKind.Utc);
        }

        public static explicit operator DateTime(BirthdayDate date)
        {
            DateTime now = DateTime.UtcNow.Date;
            bool appliedLeapFix = false;
            DateTime result;
            try
            {
                result = new DateTime(now.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException)
            {
                appliedLeapFix = true;
                result = new DateTime(now.Year, date.Month + 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
            }

            if (result < now)
            {
                result = result.AddYears(1);
                if (appliedLeapFix)
                    result = result.AddDays(-1);
            }
            return result;
        }

        public static bool operator ==(BirthdayDate left, BirthdayDate right)
            => left.Equals(right);

        public static bool operator !=(BirthdayDate left, BirthdayDate right)
            => !(left == right);

        public override string ToString()
            => $"{this.Day} {(Month)this.Month}";
    }
}
