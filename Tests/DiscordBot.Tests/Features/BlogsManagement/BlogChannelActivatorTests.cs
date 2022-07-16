using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.BlogsManagement.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.BlogsManagement
{
    [TestOf(typeof(BlogChannelActivator))]
    internal class BlogChannelActivatorTests : TestBase
    {
        private ulong activeCategoryID;
        private ulong inactiveCategoryID;
        private ITextChannel channel;

        public override void SetUp()
        {
            base.SetUp();

            activeCategoryID = base.Fixture.Create<ulong>();
            inactiveCategoryID = base.Fixture.Create<ulong>();

            channel = Substitute.For<ITextChannel>();
            channel.Id.Returns(base.Fixture.Create<ulong>());
            IGuild guild = Substitute.For<IGuild>();
            ICategoryChannel activeCategory = Substitute.For<ICategoryChannel>();
            activeCategory.Id.Returns(activeCategoryID);
            ICategoryChannel inactiveCategory = Substitute.For<ICategoryChannel>();
            inactiveCategory.Id.Returns(inactiveCategoryID);

            BlogsManagementOptions options = new BlogsManagementOptions()
            {
                ActiveBlogsCategoryID = activeCategory.Id,
                InactiveBlogsCategoryID = inactiveCategory.Id
            };
            base.Fixture.Freeze<IOptionsMonitor<BlogsManagementOptions>>().CurrentValue.Returns(options);

            guild.GetTextChannelAsync(default)
                .ReturnsForAnyArgs(Task.FromResult(channel));
            guild.GetCategoriesAsync()
                .ReturnsForAnyArgs(new[] { activeCategory, inactiveCategory });

            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(default).ReturnsForAnyArgs(guild);
        }

        [Test]
        [Category(nameof(BlogChannelActivator.ActivateBlogChannel))]
        public async Task Activating_InactiveBlogChannel_ChangesCategory()
        {
            TextChannelProperties properties = new TextChannelProperties();
            channel.ModifyAsync(Arg.Do<Action<TextChannelProperties>>(props => props.Invoke(properties)), Arg.Any<RequestOptions>()).Returns(Task.CompletedTask);
            channel.CategoryId.Returns(inactiveCategoryID);

            BlogChannelActivator service = base.Fixture.Create<BlogChannelActivator>();
            await service.ActivateBlogChannel(channel.Id, Fixture.Create<CancellationToken>());

            await channel.ReceivedWithAnyArgs(1).ModifyAsync(null, null);
            properties.CategoryId.Should().Be(activeCategoryID);
        }

        [Test]
        [Category(nameof(BlogChannelActivator.ActivateBlogChannel))]
        public async Task Activating_ActiveBlogChannel_ChangesNothing()
        {
            TextChannelProperties properties = new TextChannelProperties();
            channel.ModifyAsync(Arg.Do<Action<TextChannelProperties>>(props => props.Invoke(properties)), Arg.Any<RequestOptions>()).Returns(Task.CompletedTask);
            channel.CategoryId.Returns(activeCategoryID);

            BlogChannelActivator service = base.Fixture.Create<BlogChannelActivator>();
            await service.ActivateBlogChannel(channel.Id, Fixture.Create<CancellationToken>());

            await channel.DidNotReceiveWithAnyArgs().ModifyAsync(null, null);
            properties.CategoryId.Should().NotBe(inactiveCategoryID);
        }

        [Test]
        [Category(nameof(BlogChannelActivator.DeactivateBlogChannel))]
        public async Task Deactivating_ActiveBlogChannel_ChangesCategory()
        {
            TextChannelProperties properties = new TextChannelProperties();
            channel.ModifyAsync(Arg.Do<Action<TextChannelProperties>>(props => props.Invoke(properties)), Arg.Any<RequestOptions>()).Returns(Task.CompletedTask);
            channel.CategoryId.Returns(activeCategoryID);

            BlogChannelActivator service = base.Fixture.Create<BlogChannelActivator>();
            await service.DeactivateBlogChannel(channel.Id, Fixture.Create<CancellationToken>());

            await channel.ReceivedWithAnyArgs(1).ModifyAsync(null, null);
            properties.CategoryId.Should().Be(inactiveCategoryID);
        }

        [Test]
        [Category(nameof(BlogChannelActivator.DeactivateBlogChannel))]
        public async Task Deactivating_InactiveBlogChannel_ChangesNothing()
        {
            TextChannelProperties properties = new TextChannelProperties();
            channel.ModifyAsync(Arg.Do<Action<TextChannelProperties>>(props => props.Invoke(properties)), Arg.Any<RequestOptions>()).Returns(Task.CompletedTask);
            channel.CategoryId.Returns(inactiveCategoryID);

            BlogChannelActivator service = base.Fixture.Create<BlogChannelActivator>();
            await service.DeactivateBlogChannel(channel.Id, Fixture.Create<CancellationToken>());

            await channel.DidNotReceiveWithAnyArgs().ModifyAsync(null, null);
            properties.CategoryId.Should().NotBe(activeCategoryID);
        }
    }
}
