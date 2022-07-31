namespace DevSubmarine.DiscordBot
{
    public static class OrdinalNumber
    {
        public static string GetOrdinalString(this ulong number)
        {
            ulong remainder = number % 100;
            if (remainder != 11 && remainder != 12 && remainder != 13)
            {
                remainder = number % 10;
                if (remainder == 1)
                    return $"{number}st";
                if (remainder == 2)
                    return $"{number}nd";
                if (remainder == 3)
                    return $"{number}rd";
            }
            return $"{number}th";
        }

        public static string GetOrdinalString(this uint number)
            => GetOrdinalString((ulong)number);

        public static string GetOrdinalString(this long number)
        {
            long remainder = number % 100;
            if (remainder != 11 && remainder != 12 && remainder != 13)
            {
                remainder = number % 10;
                if (remainder == 1)
                    return $"{number}st";
                if (remainder == 2)
                    return $"{number}nd";
                if (remainder == 3)
                    return $"{number}rd";
            }
            return $"{number}th";
        }

        public static string GetOrdinalString(this int number)
            => GetOrdinalString((long)number);
    }
}
