using MongoDB.Bson.Serialization.Attributes;

namespace DevSubmarine.DiscordBot.Birthdays
{
    public struct BirthdayDate
    {
        [BsonElement("day")]
        public int Day { get; }
        [BsonElement("month")]
        public int Month { get; }

        [BsonConstructor(nameof(Day), nameof(Month))]
        private BirthdayDate(int day, int month)
        {
            this.Day = day;
            this.Month = month;
        }

        public BirthdayDate(DateTime date)
            : this(date.Day, date.Month) { }

        public BirthdayDate()
            : this(DateTime.UnixEpoch) { }

        public BirthdayDate AddDays(int days)
        {
            DateTime dt = (DateTime)this;
            return new BirthdayDate(dt.AddDays(days));
        }

        public static BirthdayDate Today
            => new BirthdayDate(DateTime.UtcNow.Date);

        public static explicit operator DateTime(BirthdayDate date)
        {
            DateTime now = DateTime.UtcNow;
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
    }
}
