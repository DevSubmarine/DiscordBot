using DevSubmarine.DiscordBot.RandomStatus.Placeholders;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(DevSubMemberCount))]
    public class DevSubMemberCountTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(DevSubMemberCount);

        private int _expectedCount;
        private IGuild _guild;

        public override void SetUp()
        {
            base.SetUp();

            this._expectedCount = base.Fixture.Create<int>();

            this._guild = Substitute.For<IGuild>();
            IReadOnlyCollection<IGuildUser> users = base.Fixture.CreateMany<IGuildUser>(this._expectedCount).ToArray();
            this._guild.GetUsersAsync().ReturnsForAnyArgs(users);
            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(default).ReturnsForAnyArgs(this._guild);
        }

        [Test]
        [Repeat(3)]
        [Category(nameof(DevSubMemberCount.GetReplacementAsync))]
        public async Task GetReplacement_ReturnsCorrectCount()
        {
            DevSubMemberCount placeholder = base.Fixture.Create<DevSubMemberCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            result.Should().Be(this._expectedCount.ToString());
        }

        [Test]
        [Repeat(3)]
        [Category(nameof(DevSubMemberCount.GetReplacementAsync))]
        public async Task GetReplacement_OnlyCalledOnce()
        {
            DevSubMemberCount placeholder = base.Fixture.Create<DevSubMemberCount>();
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            await this._guild.ReceivedWithAnyArgs(1).GetUsersAsync();
        }
    }
}
