namespace DevSubmarine.DiscordBot.RandomReactions
{
    public class WelcomeTrigger
    {
        public string RawValue { get; }
        public Regex ParsedRegex { get; }

        public WelcomeTrigger(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                throw new ArgumentException("Welcome trigger cannot be an empty string.", nameof(rawValue));

            this.RawValue = rawValue;
            this.ParsedRegex = new Regex($"^{rawValue}\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public bool IsMatch(string input)
            => this.ParsedRegex.IsMatch(input);
    }
}