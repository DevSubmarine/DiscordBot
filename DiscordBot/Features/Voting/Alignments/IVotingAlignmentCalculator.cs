namespace DevSubmarine.DiscordBot.Voting
{
    public interface IVotingAlignmentCalculator
    {
        VotingAlignment GetAlignment(double score); 
        VotingAlignment CalculateAlignment(double goodPoints, double badPoints);
    }
}
