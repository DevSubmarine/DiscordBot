using Discord;

namespace DevSubmarine.DiscordBot.RandomReactions
{
    public class RandomReactionEmote
    {
        public string RawValue { get; }
        public IEmote Emote { get; }
        public double Chance { get; }
        public RandomReactionEmote(string rawEmote, double chance)
        {
            this.RawValue = rawEmote;
            this.Chance = chance;
            if (Emoji.TryParse(rawEmote, out Emoji emoji))
                this.Emote = emoji;
            else if (Discord.Emote.TryParse(rawEmote, out Emote emote))
                this.Emote = emote;
            else
                throw new FormatException($"{rawEmote} is not a valid emote format!");
        }
        public override string ToString()
            => this.Emote.ToString();
    }
}