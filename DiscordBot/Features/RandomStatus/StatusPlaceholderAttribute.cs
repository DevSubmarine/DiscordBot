namespace DevSubmarine.DiscordBot.RandomStatus.Placeholders
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StatusPlaceholderAttribute : Attribute
    {
        public const RegexOptions DefaultRegexOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled;

        public string Placeholder { get; }
        public RegexOptions RegexOptions { get; }

        public Regex PlaceholderRegex { get; }

        public StatusPlaceholderAttribute(string placeholder, RegexOptions regexOptions)
        {
            if (string.IsNullOrWhiteSpace(placeholder))
                throw new ArgumentNullException(placeholder);

            this.Placeholder = placeholder;
            this.RegexOptions = regexOptions;
            this.PlaceholderRegex = new Regex(placeholder, regexOptions);
        }

        public StatusPlaceholderAttribute(string placeholder)
            : this(placeholder, DefaultRegexOptions) { }
    }
}
