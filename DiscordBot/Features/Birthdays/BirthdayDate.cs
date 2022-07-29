using MongoDB.Bson.Serialization.Attributes;

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

        public bool IsToday
            => this.Day == DateTime.UtcNow.Day && this.Month == DateTime.UtcNow.Month;
        public static BirthdayDate Today
            => new BirthdayDate(DateTime.UtcNow.Date);

        [BsonConstructor(nameof(Day), nameof(Month), nameof(Year))]
        public BirthdayDate(int day, int month, int? year)
        {
            if (!Validate(day, month))
                throw new ArgumentException($"{day}.{month} is not a valid date.");
            this.Day = day;
            this.Month = month;
            this.Year = year;
        }

        public BirthdayDate(DateTime date)
            : this(date.Day, date.Month, date.Year) { }

        public BirthdayDate AddDays(int days)
        {
            DateTime dt = (DateTime)this;
            return new BirthdayDate(dt.AddDays(days));
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
                DateTime date = new DateTime(2020, month, day, 0, 0, 0, DateTimeKind.Utc);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }

        public static explicit operator DateTime(BirthdayDate date)
        {
            DateTime now = DateTime.UtcNow.Date;
            bool appliedLeapFix = false;
            DateTime result;
            try
            {
                result = new DateTime(now.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                appliedLeapFix = true;
                result = new DateTime(now.Year, date.Month + 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
