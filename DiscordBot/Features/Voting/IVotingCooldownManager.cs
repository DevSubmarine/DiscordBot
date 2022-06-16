namespace DevSubmarine.DiscordBot.Voting
{
    public interface IVotingCooldownManager
    {
        void AddCooldown(ulong voterID, ulong targetID);
        bool IsReady(ulong voterID, ulong targetID, out TimeSpan cooldownRemaining);
    }
}
