using Discord;
using TehGM.Utilities.Randomization;

namespace DevSubmarine.DiscordBot
{
    public static class RandomizerExtensions
    {
        public static Color GetRandomDiscordColor(this IRandomizer randomizer)
        {
            int r = randomizer.GetRandomNumber(0, 255, true);
            int g = randomizer.GetRandomNumber(0, 255, true);
            int b = randomizer.GetRandomNumber(0, 255, true);
            return new Color(r, g, b);
        }
    }
}
