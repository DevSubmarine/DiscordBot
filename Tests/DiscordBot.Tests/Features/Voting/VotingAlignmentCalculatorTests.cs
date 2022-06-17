using DevSubmarine.DiscordBot.Voting;
using DevSubmarine.DiscordBot.Voting.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.Voting
{
    [TestOf(typeof(VotingAlignmentCalculator))]
    public class VotingAlignmentCalculatorTests : TestBase
    {
        private IDictionary<VotingAlignmentLevel, string> _images;

        public override void SetUp()
        {
            base.SetUp();

            this._images = new Dictionary<VotingAlignmentLevel, string>()
            {
                { VotingAlignmentLevel.ChaoticEvil, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.NeutralEvil, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.LawfulEvil, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.ChaoticNeutral, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.TrueNeutral, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.LawfulNeutral, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.ChaoticGood, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.NeutralGood, base.Fixture.Create<string>() },
                { VotingAlignmentLevel.LawfulGood, base.Fixture.Create<string>() }
            };
            VotingOptions options = new VotingOptions() { AlignmentImages = this._images };
            base.Fixture.Freeze<IOptionsMonitor<VotingOptions>>().CurrentValue.Returns(options);
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(VotingAlignmentCalculator.GetAlignment))]
        public void Alignment_ReturnsValidScore(double score)
        {
            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();

            VotingAlignment result = service.GetAlignment(score);

            result.Should().NotBeNull();
            result.Score.Should().Be(score);
        }

        [Test]
        [TestCase(10, VotingAlignmentLevel.ChaoticEvil)]
        [TestCase(20, VotingAlignmentLevel.NeutralEvil)]
        [TestCase(30, VotingAlignmentLevel.LawfulEvil)]
        [TestCase(40, VotingAlignmentLevel.ChaoticNeutral)]
        [TestCase(50, VotingAlignmentLevel.TrueNeutral)]
        [TestCase(60, VotingAlignmentLevel.LawfulNeutral)]
        [TestCase(70, VotingAlignmentLevel.ChaoticGood)]
        [TestCase(80, VotingAlignmentLevel.NeutralGood)]
        [TestCase(90, VotingAlignmentLevel.LawfulGood)]
        [Category(nameof(VotingAlignmentCalculator.GetAlignment))]
        public void Alignment_ReturnsMatchingLevel(double score, VotingAlignmentLevel expectedResult)
        {
            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();

            VotingAlignment result = service.GetAlignment(score);

            result.Should().NotBeNull();
            result.Level.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(10, VotingAlignmentLevel.ChaoticEvil)]
        [TestCase(20, VotingAlignmentLevel.NeutralEvil)]
        [TestCase(30, VotingAlignmentLevel.LawfulEvil)]
        [TestCase(40, VotingAlignmentLevel.ChaoticNeutral)]
        [TestCase(50, VotingAlignmentLevel.TrueNeutral)]
        [TestCase(60, VotingAlignmentLevel.LawfulNeutral)]
        [TestCase(70, VotingAlignmentLevel.ChaoticGood)]
        [TestCase(80, VotingAlignmentLevel.NeutralGood)]
        [TestCase(90, VotingAlignmentLevel.LawfulGood)]
        [Category(nameof(VotingAlignmentCalculator.GetAlignment))]
        public void Alignment_ReturnsMatchingImageURL(double score, VotingAlignmentLevel level)
        {
            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();
            string expectedResult = this._images[level];

            VotingAlignment result = service.GetAlignment(score);

            result.Should().NotBeNull();
            result.ImageURL.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(1, 4, 20)]
        [TestCase(3, 2, 60)]
        [Category(nameof(VotingAlignmentCalculator.CalculateAlignment))]
        public void Calculating_RawPoints_ReturnsCorrectScore(double goodPoints, double badPoints, double expectedScore)
        {
            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();

            VotingAlignment result = service.CalculateAlignment(goodPoints, badPoints);

            result.Should().NotBeNull();
            result.Score.Should().Be(expectedScore);
        }

        [Test]
        [TestCase(1, 4, 20)]
        [TestCase(3, 2, 60)]
        [Category(nameof(VotingAlignmentCalculatorExtensions.CalculateAlignment))]
        public void Calculating_TwoVoteCollections_ReturnsCorrectScore(int goodVotesCount, int badVotesCount, double expectedScore)
        {
            ICollection<Vote> goodVotes = new List<Vote>();
            goodVotes.AddMany(() => new Vote(VoteType.Mod, base.Fixture.Create<ulong>(), base.Fixture.Create<ulong>()), goodVotesCount);
            ICollection<Vote> badVotes = new List<Vote>();
            badVotes.AddMany(() => new Vote(VoteType.Kick, base.Fixture.Create<ulong>(), base.Fixture.Create<ulong>()), badVotesCount);

            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();

            VotingAlignment result = service.CalculateAlignment(goodVotes, badVotes);

            result.Should().NotBeNull();
            result.Score.Should().Be(expectedScore);
        }

        [Test]
        [TestCase(1, 4, 20)]
        [TestCase(3, 2, 60)]
        [Category(nameof(VotingAlignmentCalculatorExtensions.CalculateAlignment))]
        public void Calculating_OneVoteCollection_ReturnsCorrectScore(int goodVotesCount, int badVotesCount, double expectedScore)
        {
            ICollection<Vote> votes = new List<Vote>();
            votes.AddMany(() => new Vote(VoteType.Mod, base.Fixture.Create<ulong>(), base.Fixture.Create<ulong>()), goodVotesCount);
            votes.AddMany(() => new Vote(VoteType.Kick, base.Fixture.Create<ulong>(), base.Fixture.Create<ulong>()), badVotesCount);

            VotingAlignmentCalculator service = base.Fixture.Create<VotingAlignmentCalculator>();

            VotingAlignment result = service.CalculateAlignment(votes);

            result.Should().NotBeNull();
            result.Score.Should().Be(expectedScore);
        }
    }
}
