namespace DevSubmarine.DiscordBot.RandomReactions
{
    public interface IWelcomeTriggerProvider
    {
        IEnumerable<WelcomeTrigger> GetWelcomeTriggers();
    }

    public static class WelcomeTriggerProviderExtensions
    {
        public static bool IsAnyMatching(this IWelcomeTriggerProvider provider, string input)
            => provider.GetWelcomeTriggers().Any(trigger => trigger.IsMatch(input));
    }
}