namespace DevSubmarine.DiscordBot.Voting
{
    /// <summary>Calculates DnD-style alignment based on user's voting history.</summary>
    public interface IVotingAlignmentCalculator
    {
        /// <summary>Gets alignment information based on raw score.</summary>
        /// <param name="score">Alignment score. Should be between 0 and 100.</param>
        /// <returns>Alignment information.</returns>
        VotingAlignment GetAlignment(double score);
        /// <summary>Calculates alignment score and gets alignment information.</summary>
        /// <param name="goodPoints">The amount of "good" points.</param>
        /// <param name="badPoints">The amount of "bad" points.</param>
        /// <returns>Alignment information.</returns>
        VotingAlignment CalculateAlignment(double goodPoints, double badPoints);
    }
}
