namespace DevSubmarine.DiscordBot.RandomReactions
{
    public interface IRandomReactionEmoteProvider
    {
        IEnumerable<RandomReactionEmote> GetWelcomeEmotes();
        IEnumerable<RandomReactionEmote> GetFollowupEmotes();
        IEnumerable<RandomReactionEmote> GetRandomEmotes();
    }
}