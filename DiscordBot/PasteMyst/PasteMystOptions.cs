namespace DevSubmarine.DiscordBot.PasteMyst
{
    public class PasteMystOptions
    {
        public string UserAgent { get; set; } = $"DevSubmarine's DiscordBot v{AppVersion.Version}";
        public string AuthorizationToken { get; set; } = null;
    }
}
