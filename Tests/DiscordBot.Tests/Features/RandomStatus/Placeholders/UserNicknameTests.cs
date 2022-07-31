using DevSubmarine.DiscordBot.RandomStatus.Placeholders;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    [TestOf(typeof(UserNickname))]
    public class UserNicknameTests : PlaceholderTestBase
    {
        protected override Type PlaceholderType => typeof(UserNickname);
        private const string _placeholderName = "UserNickname";

        private IGuild _guild;

        public override void SetUp()
        {
            base.SetUp();

            this._guild = Substitute.For<IGuild>();
            this._guild.GetUserAsync(default).ReturnsForAnyArgs((IGuildUser)null);
            base.Fixture.Freeze<IDiscordClient>().GetGuildAsync(default).ReturnsForAnyArgs(this._guild);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public async Task GetReplacement_GuildMember_WithNickname_ReturnsNickname(ulong userID, string username, string nickname)
        {
            this.BuildGuildUser(userID, username, nickname);

            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            result.Should().Be(nickname);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public async Task GetReplacement_GuildMember_WithoutNickname_ReturnsUsername(ulong userID, string username)
        {
            this.BuildGuildUser(userID, username, null);

            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            result.Should().Be(username);
        }

        [Test, AutoNSubstituteData]
        [Repeat(3)]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public async Task GetReplacement_NotGuildMember_ReturnsUsername(ulong userID, string username)
        {
            this.BuildUser(userID, username);

            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            string result = await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            result.Should().Be(username);
        }

        [Test]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public void GetReplacement_NoID_Throws()
        {
            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName));

            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Placeholder requires a valid user ID to be provided");
        }

        [Test]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public void GetReplacement_InvalidID_Throws()
        {
            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, "foobar"));

            act.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Placeholder: foobar is not a valid user ID");
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public void GetReplacement_UserNotFound_Throws(ulong userID)
        {
            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            Func<Task> act = async () => await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Discord user with ID {userID} not found");
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public async Task GetReplacement_GuildMember_OnlyCalledOnce(ulong userID, string username, string nickname)
        {
            this.BuildGuildUser(userID, username, nickname);

            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            await this._guild.ReceivedWithAnyArgs(1).GetUserAsync(default);
        }

        [Test, AutoNSubstituteData]
        [Category(nameof(UserNickname.GetReplacementAsync))]
        public async Task GetReplacement_NotGuildMember_OnlyCalledOnce(ulong userID, string username)
        {
            this.BuildUser(userID, username);

            UserNickname placeholder = base.Fixture.Create<UserNickname>();
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));
            await placeholder.GetReplacementAsync(base.CreateTestMatch(_placeholderName, userID));

            await this._guild.ReceivedWithAnyArgs(1).GetUserAsync(default);
        }

        private IUser BuildUser(ulong userID, string username)
        {
            IUser user = Substitute.For<IUser>();
            user.Id.Returns(userID);
            user.Username.Returns(username);
            base.Fixture.Freeze<IDiscordClient>().GetUserAsync(userID, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(user);
            return user;
        }

        private IGuildUser BuildGuildUser(ulong userID, string username, string nickname)
        {
            this.BuildUser(userID, username);

            IGuildUser user = Substitute.For<IGuildUser>();
            user.Id.Returns(userID);
            user.Username.Returns(username);
            user.Nickname.Returns(nickname);
            this._guild.GetUserAsync(userID, Arg.Any<CacheMode>(), Arg.Any<RequestOptions>()).Returns(user);
            return user;
        }
    }
}
