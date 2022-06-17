namespace DevSubmarine.DiscordBot.Voting
{
    public enum VotingAlignmentLevel
    {
        ChaoticEvil,
        NeutralEvil,
        LawfulEvil,
        ChaoticNeutral,
        TrueNeutral,
        LawfulNeutral,
        ChaoticGood,
        NeutralGood,
        LawfulGood
    }

    public static class VotingAlignmentLevelExtensions
    {
        public static string GetText(this VotingAlignmentLevel level)
        {
            switch (level)
            {
                case VotingAlignmentLevel.ChaoticEvil:
                    return "Chaotic Evil";
                case VotingAlignmentLevel.NeutralEvil:
                    return "Neutral Evil";
                case VotingAlignmentLevel.LawfulEvil:
                    return "Lawful Evil";
                case VotingAlignmentLevel.ChaoticNeutral:
                    return "Chaotic Neutral";
                case VotingAlignmentLevel.TrueNeutral:
                    return "True Neutral";
                case VotingAlignmentLevel.LawfulNeutral:
                    return "Lawful Neutral";
                case VotingAlignmentLevel.ChaoticGood:
                    return "Chaotic Good";
                case VotingAlignmentLevel.NeutralGood:
                    return "Neutral Good";
                case VotingAlignmentLevel.LawfulGood:
                    return "Lawful Good";
                default:
                    return level.ToString();
            }
        }
    }
}
