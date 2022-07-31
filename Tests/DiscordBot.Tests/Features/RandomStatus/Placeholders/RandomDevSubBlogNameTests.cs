using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using TehGM.Utilities.Randomization;
using TehGM.Utilities.Randomization.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(RandomDevSubBlogName))]
    public class RandomDevSubBlogNameTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(RandomDevSubBlogName);
        private const string _placeholderName = "RandomDevSubBlogName";

        private BlogsManagementOptions _blogOptions;

        public override void SetUp()
        {
            base.SetUp();

            this._blogOptions = new BlogsManagementOptions();
            this._blogOptions.ActiveBlogsCategoryID = base.Fixture.Create<ulong>();
            this._blogOptions.InactiveBlogsCategoryID = base.Fixture.Create<ulong>();

            base.Fixture.Freeze<IOptionsSnapshot<BlogsManagementOptions>>().Value.Returns(this._blogOptions);
            base.Fixture.Register<IRandomizer>(() => new RandomizerService());
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_NoTag_ReturnsRandomChannel(IEnumerable<string> activeChannels, IEnumerable<string> inactiveChannels)
        {
            this.BuildGuild(activeChannels, inactiveChannels, Enumerable.Empty<string>(), Enumerable.Empty<string>());
            IEnumerable<string> validResults = activeChannels.Union(inactiveChannels);

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName));

            result.Should().BeOneOf(validResults);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_NoTag_IgnoresInactiveChannels(IEnumerable<string> ignoredActiveChannels, IEnumerable<string> ignoredInactiveChannels)
        {
            this.BuildGuild(Enumerable.Empty<string>(), Enumerable.Empty<string>(), ignoredActiveChannels, ignoredInactiveChannels);

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName));

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_ActiveTag_ReturnsCorrectCount(IEnumerable<string> activeChannels, IEnumerable<string> inactiveChannels)
        {
            this.BuildGuild(activeChannels, inactiveChannels, Enumerable.Empty<string>(), Enumerable.Empty<string>());

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "active"));

            result.Should().BeOneOf(activeChannels);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_ActiveTag_IgnoresInactiveChannels(IEnumerable<string> ignoredActiveChannels, IEnumerable<string> ignoredInactiveChannels)
        {
            this.BuildGuild(Enumerable.Empty<string>(), Enumerable.Empty<string>(), ignoredActiveChannels, ignoredInactiveChannels);

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "active"));

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_InactiveTag_ReturnsCorrectCount(IEnumerable<string> activeChannels, IEnumerable<string> inactiveChannels)
        {
            this.BuildGuild(activeChannels, inactiveChannels, Enumerable.Empty<string>(), Enumerable.Empty<string>());

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "inactive"));

            result.Should().BeOneOf(inactiveChannels);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_InactiveTag_IgnoresInactiveChannels(IEnumerable<string> ignoredActiveChannels, IEnumerable<string> ignoredInactiveChannels)
        {
            this.BuildGuild(Enumerable.Empty<string>(), Enumerable.Empty<string>(), ignoredActiveChannels, ignoredInactiveChannels);

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "inactive"));

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(RandomDevSubBlogName.GetReplacementAsync))]
        public async Task GetReplacement_CalledMultipleTimes(IEnumerable<string> activeChannels, IEnumerable<string> inactiveChannels, IEnumerable<string> ignoredActiveChannels, IEnumerable<string> ignoredInactiveChannels)
        {
            IGuild guild = this.BuildGuild(activeChannels, inactiveChannels, ignoredActiveChannels, ignoredInactiveChannels);

            RandomDevSubBlogName placeholder = base.Fixture.Create<RandomDevSubBlogName>();
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName));
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName));

            await guild.Received(2).GetTextChannelsAsync(Arg.Any<CacheMode>(), Arg.Any<RequestOptions>());
        }

        private IGuild BuildGuild(IEnumerable<string> activeChannels, IEnumerable<string> inactiveChannels, IEnumerable<string> ignoredActiveChannels, IEnumerable<string> ignoredInactiveChannels)
        {
            IGuild guild = Substitute.For<IGuild>();
            List<ITextChannel> channels = new List<ITextChannel>(activeChannels.Count() + inactiveChannels.Count() + ignoredActiveChannels.Count() + ignoredInactiveChannels.Count());
            List<ITextChannel> ignoredChannels = new List<ITextChannel>(ignoredActiveChannels.Count() + ignoredInactiveChannels.Count());

            CreateChannels(active: true, ignored: false, activeChannels);
            CreateChannels(active: true, ignored: true, ignoredActiveChannels);
            CreateChannels(active: false, ignored: false, inactiveChannels);
            CreateChannels(active: false, ignored: true, ignoredInactiveChannels);

            guild.GetChannelsAsync().ReturnsForAnyArgs(channels);
            guild.GetTextChannelsAsync().ReturnsForAnyArgs(channels);
            this._blogOptions.IgnoredChannelsIDs = ignoredChannels.Select(ch => ch.Id);

            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(default).ReturnsForAnyArgs(guild);
            return guild;

            void CreateChannels(bool active, bool ignored, IEnumerable<string> names)
            {
                foreach (string name in names)
                {
                    ITextChannel channel = Substitute.For<ITextChannel>();
                    channel.Id.Returns(base.Fixture.Create<ulong>());
                    channel.Name.Returns(name);
                    channel.CategoryId.Returns(active ? this._blogOptions.ActiveBlogsCategoryID : this._blogOptions.InactiveBlogsCategoryID);
                    channels.Add(channel);
                    if (ignored)
                        ignoredChannels.Add(channel);
                }
            }
        }
    }
}
