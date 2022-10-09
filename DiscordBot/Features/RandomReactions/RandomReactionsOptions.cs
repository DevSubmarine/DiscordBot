using Discord;

namespace DevSubmarine.DiscordBot.RandomReactions
{
    public class RandomReactionsOptions
    {
        // followups - happen if a message already contains the emote, or has a reaction with that emote.
        public IDictionary<string, double> FollowupEmotes { get; set; } = new Dictionary<string, double>()
        {
            { ResponseEmoji.PepeComfy, 0.2 },
            { ResponseEmoji.Pensive, 0.1 },
            { ResponseEmoji.FeelsCoffeeMan, 0.2 },
            { ResponseEmoji.Hype, 0.05 },
            { ResponseEmoji.IAmAgony, 0.05 },
            { ResponseEmoji.Despair, 0.1 }
        };

        // randoms - can happen fully randomly, no matter the message
        public IDictionary<string, double> RandomEmotes { get; set; } = new Dictionary<string, double>()
        {
            { ResponseEmoji.PepeComfy, 0.005 },
            { ResponseEmoji.Pensive, 0.001 },
            { ResponseEmoji.FeelsCoffeeMan, 0.005 },
            { ResponseEmoji.Hype, 0.001 },
            { ResponseEmoji.IAmAgony, 0.001 },
            { ResponseEmoji.Despair, 0.002 }
        };

        // welcome - when someone says hi etc... these are quite message-content sensitive, so will work wierdly in many cases, but it's okay...
        public IDictionary<string, double> WelcomeEmotes { get; set; } = new Dictionary<string, double>()
        {
            { ResponseEmoji.Wave1, 0.3 }
        };
        public IEnumerable<string> WelcomeTriggers { get; set; } = new string[]
        {
            "welcome", "hello", "hi", "henlo", "hey", "hlo"
        };
    }
}
