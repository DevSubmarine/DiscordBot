namespace DevSubmarine.DiscordBot.BlogsManagement.Services
{
    /// <inheritdoc/>
    internal class BlogChannelNameConverter : IBlogChannelNameConverter
    {
        private readonly BlogsManagementOptions _options;

        public BlogChannelNameConverter(IOptionsMonitor<BlogsManagementOptions> options)
        {
            this._options = options.CurrentValue;
        }

        /// <inheritdoc/>
        public bool IsUsernameAllowed(string username)
            => this.IsUsernameAllowedInternal(this.Normalize(username));

        /// <inheritdoc/>
        public string ConvertUsername(string username)
        {
            string normalizedUsername = this.Normalize(username);

            if (!this.IsUsernameAllowed(normalizedUsername))
                throw new ArgumentException($"Username '{username}' violates some validation rules", username);

            return $"blog-{normalizedUsername}";
        }

        private bool IsUsernameAllowedInternal(string normalizedUsername)
        {
            if (normalizedUsername.StartsWith("blog", StringComparison.OrdinalIgnoreCase))
                return false;
            if (normalizedUsername.StartsWith("chat", StringComparison.OrdinalIgnoreCase))
                return false;
            if (this._options.ForbiddenChannelNameWords.Any(word => normalizedUsername.Contains(word)))
                return false;

            return true;
        }

        private string Normalize(string value)
            => value
                .ToLowerInvariant()
                .Replace('_', '-')
                .Replace(".", "-")
                .Trim('-');
    }
}
