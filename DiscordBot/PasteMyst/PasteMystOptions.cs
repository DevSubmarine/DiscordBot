namespace DevSubmarine.DiscordBot.PasteMyst
{
    public class PasteMystOptions
    {
        /// <summary>User agent to use when making requests to PasteMyst.</summary>
        public string UserAgent { get; set; } = $"DevSubmarine's DiscordBot v{AppVersion.Version}";
        /// <summary>Auth token to use for PasteMyst requests.</summary>
        /// <remarks>If left null, requests will be made without logging in.</remarks>
        public string AuthorizationToken { get; set; } = null;
    }
}
