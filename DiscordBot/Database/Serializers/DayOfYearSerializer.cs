using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace DevSubmarine.DiscordBot.Database.Serializers
{
    public class DayOfYearSerializer : SerializerBase<DateTime>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            context.Writer.WriteInt32(value.DayOfYear);
        }

        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            int dayOfYear = context.Reader.ReadInt32();
            DateTime result = new DateTime(DateTime.UtcNow.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddDays(dayOfYear).Date;
            return UpdatePastDate(result);
        }

        // we don't care about past birthdays, so add one year for those
        public static DateTime UpdatePastDate(DateTime date)
        {
            while (date.Date < DateTime.UtcNow.Date)
                date = date.AddYears(1).Date;
            return date;
        }
    }
}
