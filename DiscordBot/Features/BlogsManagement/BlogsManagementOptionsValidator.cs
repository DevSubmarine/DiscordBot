namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <summary>Validates instances of <see cref="BlogsManagementOptions"/>.</summary>
    public class BlogsManagementOptionsValidator : IValidateOptions<BlogsManagementOptions>
    {
        /// <inheritdoc/>
        public ValidateOptionsResult Validate(string name, BlogsManagementOptions options)
        {
            if (options.ActiveBlogsCategoryID == default)
                return ValidateOptionsResult.Fail($"{nameof(options.ActiveBlogsCategoryID)} is required.");
            if (options.InactiveBlogsCategoryID == default)
                return ValidateOptionsResult.Fail($"{nameof(options.InactiveBlogsCategoryID)} is required.");

            return ValidateOptionsResult.Success;
        }
    }
}
