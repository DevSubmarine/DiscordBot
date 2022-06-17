namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Service that manages voting cooldown.</summary>
    public interface IVotingCooldownManager
    {
        /// <summary>Starts cooldown for the vote.</summary>
        /// <param name="voterID">ID of user that did the vote.</param>
        /// <param name="targetID">ID of the user that was target of the vote.</param>
        void AddCooldown(ulong voterID, ulong targetID);
        /// <summary>Checks whether the vote can be made or is in cooldown.</summary>
        /// <param name="voterID">ID of user that did the vote.</param>
        /// <param name="targetID">ID of the user that was target of the vote.</param>
        /// <param name="cooldownRemaining">Cooldown remaining, if any.</param>
        /// <returns>True if the vote can be made; false if it's still in cooldown.</returns>
        bool IsReady(ulong voterID, ulong targetID, out TimeSpan cooldownRemaining);
    }
}
