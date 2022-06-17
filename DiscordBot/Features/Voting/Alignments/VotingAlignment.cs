using System.Globalization;

namespace DevSubmarine.DiscordBot.Voting
{
    public class VotingAlignment
    {
        public double Score { get; }
        public VotingAlignmentLevel Level { get; }
        public string ImageURL { get; }

        public VotingAlignment(double score, VotingAlignmentLevel level, string imageURL)
        {
            this.Score = score;
            this.Level = level;
            this.ImageURL = imageURL;
        }

        public override string ToString()
            => $"{this.Level.GetText()} ({FormatScore(this.Score)})";

        public static string FormatScore(double score)
            => score.ToString("0.#", CultureInfo.InvariantCulture) + "%";
    }
}
