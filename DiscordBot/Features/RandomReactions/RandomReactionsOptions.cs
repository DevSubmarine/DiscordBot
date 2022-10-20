namespace DevSubmarine.DiscordBot.RandomReactions
{
    public class RandomReactionsOptions
    {
        public bool Enabled { get; set; } = true;

        // followups - happen if a message already contains the emote, or has a reaction with that emote.
        public IEnumerable<EmoteOptions> FollowupEmotes { get; set; }

        // randoms - can happen fully randomly, no matter the message
        public IEnumerable<EmoteOptions>RandomEmotes { get; set; }

        // welcome - when someone says hi etc... these are quite message-content sensitive, so will work wierdly in many cases, but it's okay...
        public IEnumerable<EmoteOptions> WelcomeEmotes { get; set; }
        public IEnumerable<string> WelcomeTriggers { get; set; }

        public class EmoteOptions
        {
            public string Emote { get; set; }
            public double Chance { get; set; }
        }
    }
}
