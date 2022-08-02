namespace DevSubmarine.DiscordBot
{
    /// <summary>Validates instances of <see cref="DevSubOptions"/>.</summary>
    public class DevSubOptionsValidator : IValidateOptions<DevSubOptions>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, DevSubOptions options)
        {
            if (options.GuildID <= 0)
                return ValidateOptionsResult.Fail($"DevSub {nameof(options.GuildID)} needs to be configured.");

            return ValidateOptionsResult.Success;
        }
    }
}
