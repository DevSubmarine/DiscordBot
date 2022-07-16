using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using TehGM.Utilities.Randomization;
using TehGM.Utilities.Randomization.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(RandomDevSubMemberName))]
    public class RandomDevSubMemberNameTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(RandomDevSubMemberName);

        public override void SetUp()
        {
            base.SetUp();

            base.Fixture.Register<IRandomizer>(() => new RandomizerService());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public async Task GetReplacement_WithNicknames_ReturnsRandomUsername(IDictionary<string, string> users)
        {
            this.CreateGuild(users);
            IEnumerable<string> validResults = users.Values;

            RandomDevSubMemberName placeholder = base.Fixture.Create<RandomDevSubMemberName>();
            string result = await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            result.Should().BeOneOf(validResults);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public async Task GetReplacement_WithoutNicknames_ReturnsRandomUsername(IEnumerable<string> users)
        {
            this.CreateGuild(users.ToDictionary<string, string, string>(u => u, _ => null));
            IEnumerable<string> validResults = users;

            RandomDevSubMemberName placeholder = base.Fixture.Create<RandomDevSubMemberName>();
            string result = await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            result.Should().BeOneOf(validResults);
        }

        [Test]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public async Task GetReplacement_NoUsersFound_Throws()
        {
            this.CreateGuild(Enumerable.Empty<KeyValuePair<string, string>>());

            RandomDevSubMemberName placeholder = base.Fixture.Create<RandomDevSubMemberName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public async Task GetReplacement_CalledMultipleTimes(IDictionary<string, string> users)
        {
            IGuild guild = this.CreateGuild(users);

            RandomDevSubMemberName placeholder = base.Fixture.Create<RandomDevSubMemberName>();
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch());

            await guild.Received(2).GetUsersAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>());
        }

        private IGuild CreateGuild(IEnumerable<KeyValuePair<string, string>> usernamesAndNicknames)
        {
            IGuild guild = Substitute.For<IGuild>();
            guild.GetUsersAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>())
                .Returns(x => usernamesAndNicknames.Select(u =>
                {
                    IGuildUser user = Substitute.For<IGuildUser>();
                    user.Username.Returns(u.Key);
                    user.Nickname.Returns(u.Value);
                    return user;
                }).ToArray());
            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(Arg.Any<ulong>(), Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(guild);
            return guild;
        }
    }
}
