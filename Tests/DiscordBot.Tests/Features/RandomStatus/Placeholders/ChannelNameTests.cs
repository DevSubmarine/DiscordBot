using DevSubmarine.DiscordBot.RandomStatus.Placeholders;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(ChannelName))]
    internal class ChannelNameTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(ChannelName);
        private const string _placeholderName = "ChannelName";

        public override void SetUp()
        {
            base.SetUp();

            base.Fixture.Freeze<IDiscordClient>().GetChannelAsync(Arg.Any<ulong>(), Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns((IChannel)null);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public async Task GetReplacement_ReturnsChannelName(ulong id, string name)
        {
            this.CreateChannel(id, name);

            ChannelName placeholder = base.Fixture.Create<ChannelName>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, id));

            result.Should().Be(name);
        }


        [Test]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public void GetReplacement_NoID_Throws()
        {
            ChannelName placeholder = base.Fixture.Create<ChannelName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName), default);

            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Placeholder requires a valid channel ID to be provided");
        }

        [Test]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public void GetReplacement_InvalidID_Throws()
        {
            ChannelName placeholder = base.Fixture.Create<ChannelName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "foobar"), default);

            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Placeholder: foobar is not a valid channel ID");
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(ChannelName.GetReplacementAsync))]
        public void GetReplacement_ChannelNotFound_Throws(ulong id)
        {
            ChannelName placeholder = base.Fixture.Create<ChannelName>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, id), default);

            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Discord channel with ID {id} not found");
        }

        private IChannel CreateChannel(ulong id, string name)
        {
            IChannel channel = Substitute.For<IChannel>();
            channel.Id.Returns(id);
            channel.Name.Returns(name);
            base.Fixture.Freeze<IDiscordClient>().GetChannelAsync(id, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(channel);
            return channel;
        }
    }
}
