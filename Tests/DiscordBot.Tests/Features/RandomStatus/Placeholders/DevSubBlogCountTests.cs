using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.RandomStatus.Placeholders;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(DevSubBlogCount))]
    public class DevSubBlogCountTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(DevSubBlogCount);
        private const string _placeholderName = "DevSubBlogCount";

        private BlogsManagementOptions _blogOptions;

        public override void SetUp()
        {
            base.SetUp();

            this._blogOptions = new BlogsManagementOptions();
            this._blogOptions.ActiveBlogsCategoryID = base.Fixture.Create<ulong>();
            this._blogOptions.InactiveBlogsCategoryID = base.Fixture.Create<ulong>();

            base.Fixture.Freeze<IOptionsSnapshot<BlogsManagementOptions>>().Value.Returns(this._blogOptions);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_NoTag_ReturnsCorrectCount(int activeCount, int inactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, 0, 0);
            int expectedResult = activeCount + inactiveCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch(), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_NoTag_IgnoresInactiveChannels(int activeCount, int inactiveCount, int ignoredActiveCount, int ignoredInactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, ignoredActiveCount, ignoredInactiveCount);
            int expectedResult = activeCount + inactiveCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch(), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_ActiveTag_ReturnsCorrectCount(int activeCount, int inactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, 0, 0);
            int expectedResult = activeCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "active"), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_ActiveTag_IgnoresInactiveChannels(int activeCount, int inactiveCount, int ignoredActiveCount, int ignoredInactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, ignoredActiveCount, ignoredInactiveCount);
            int expectedResult = activeCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "active"), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_InactiveTag_ReturnsCorrectCount(int activeCount, int inactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, 0, 0);
            int expectedResult = inactiveCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "inactive"), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_InactiveTag_IgnoresInactiveChannels(int activeCount, int inactiveCount, int ignoredActiveCount, int ignoredInactiveCount)
        {
            this.BuildGuild(activeCount, inactiveCount, ignoredActiveCount, ignoredInactiveCount);
            int expectedResult = inactiveCount;

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "inactive"), default);

            result.Should().Be(expectedResult.ToString());
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(DevSubBlogCount.GetReplacementAsync))]
        public async Task GetReplacement_OnlyCalledOnce(int activeCount, int inactiveCount, int ignoredActiveCount, int ignoredInactiveCount)
        {
            IGuild guild = this.BuildGuild(activeCount, inactiveCount, ignoredActiveCount, ignoredInactiveCount);

            DevSubBlogCount placeholder = base.Fixture.Create<DevSubBlogCount>();
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch(), default);
            await placeholder.GetReplacementAsync(base.CreateDefaultTestMatch(), default);

            await guild.Received(1).GetTextChannelsAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>());
        }

        private IGuild BuildGuild(int activeCount, int inactiveCount, int ignoredActiveCount, int ignoredInactiveCount)
        {
            IGuild guild = Substitute.For<IGuild>();
            List<ITextChannel> channels = new List<ITextChannel>(activeCount + inactiveCount + ignoredActiveCount + ignoredInactiveCount);
            List<ITextChannel> ignoredChannels = new List<ITextChannel>(ignoredActiveCount + ignoredInactiveCount);

            CreateChannels(active: true, ignored: false, activeCount);
            CreateChannels(active: true, ignored: true, ignoredActiveCount);
            CreateChannels(active: false, ignored: false, inactiveCount);
            CreateChannels(active: false, ignored: true, ignoredInactiveCount);

            guild.GetChannelsAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(channels);
            guild.GetTextChannelsAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(channels);
            this._blogOptions.IgnoredChannelsIDs = ignoredChannels.Select(ch => ch.Id);

            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(Arg.Any<ulong>(), Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(guild);
            return guild;

            void CreateChannels(bool active, bool ignored, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    ITextChannel channel = Substitute.For<ITextChannel>();
                    channel.Id.Returns(base.Fixture.Create<ulong>());
                    channel.CategoryId.Returns(active ? this._blogOptions.ActiveBlogsCategoryID : this._blogOptions.InactiveBlogsCategoryID);
                    channels.Add(channel);
                    if (ignored)
                        ignoredChannels.Add(channel);
                }
            }
        }
    }
}
