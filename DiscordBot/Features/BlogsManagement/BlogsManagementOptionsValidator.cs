namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <summary>Validates instances of <see cref="BlogsManagementOptions"/>.</summary>
    public class BlogsManagementOptionsValidator : IValidateOptions<BlogsManagementOptions>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, BlogsManagementOptions options)
        {
            if (options.GuildID == default)
                return ValidateOptionsResult.Fail($"{nameof(options.GuildID)} is required.");
            if (options.ActiveBlogsCategoryID == default)
                return ValidateOptionsResult.Fail($"{nameof(options.ActiveBlogsCategoryID)} is required.");
            if (options.InactiveBlogsCategoryID == default)
                return ValidateOptionsResult.Fail($"{nameof(options.InactiveBlogsCategoryID)} is required.");

            if (options.ActivityScanningRate < TimeSpan.Zero)
                return ValidateOptionsResult.Fail($"{nameof(options.ActivityScanningRate)} must be greater than 0.");
            if (options.MaxBlogInactivityTime <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail($"{nameof(options.MaxBlogInactivityTime)} cannot be less than 0.");
            if (options.MinMemberAge <= TimeSpan.Zero)
                return ValidateOptionsResult.Fail($"{nameof(options.MinMemberAge)} cannot be less than 0.");

            return ValidateOptionsResult.Success;
        }
    }
}
