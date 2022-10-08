namespace DevSubmarine.DiscordBot.Time
{
    /// <summary>Validates instances of <see cref="DevSubOptions"/>.</summary>
    public class TimezoneOptionsValidator : IValidateOptions<TimezoneOptions>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, TimezoneOptions options)
        {
            if (options.SerializedTimezones?.Any() != true && !options.FallbackToSystemTimezones)
                return ValidateOptionsResult.Fail($"{nameof(options.SerializedTimezones)} needs to be configured, or {nameof(options.FallbackToSystemTimezones)} needs to be allowed.");

            return ValidateOptionsResult.Success;
        }
    }
}
