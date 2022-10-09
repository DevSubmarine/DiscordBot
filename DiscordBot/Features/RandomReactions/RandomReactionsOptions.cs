namespace DevSubmarine.DiscordBot.RandomReactions
{
    public class RandomReactionsOptions
    {
        public bool Enabled { get; set; } = true;

        // followups - happen if a message already contains the emote, or has a reaction with that emote.
        public IDictionary<string, double> FollowupEmotes { get; set; } = new Dictionary<string, double>();

        // randoms - can happen fully randomly, no matter the message
        public IDictionary<string, double> RandomEmotes { get; set; } = new Dictionary<string, double>();

        // welcome - when someone says hi etc... these are quite message-content sensitive, so will work wierdly in many cases, but it's okay...
        public IDictionary<string, double> WelcomeEmotes { get; set; } = new Dictionary<string, double>();
        public IEnumerable<string> WelcomeTriggers { get; set; }
    }
}
