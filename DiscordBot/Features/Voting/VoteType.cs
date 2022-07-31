namespace DevSubmarine.DiscordBot.Voting
{
    public enum VoteType : uint
    {
        Kick = 1 << 0,
        Ban = 1 << 1,
        Mod = 1 << 2
    }

    public static class VoteTypeExtensions
    {
        public static string GetText(this VoteType vote)
        {
            switch (vote)
            {
                case VoteType.Kick:
                    return "Kick";
                case VoteType.Ban:
                    return "Ban";
                case VoteType.Mod:
                    return "Mod";
                default:
                    return vote.ToString();
            }
        }

        public static bool IsPositive(this VoteType vote)
            => vote == VoteType.Mod;

        public static bool IsNegative(this VoteType vote)
            => vote == VoteType.Kick || vote == VoteType.Ban;
    }
}
