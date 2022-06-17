using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.BlogsManagement.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.BlogsManagement
{
    [TestOf(typeof(BlogChannelNameConverter))]
    public class BlogChannelNameConverterTests : TestBase
    {
        [Test]
        [TestCase("username")]
        [TestCase("two words")]
        [TestCase("with-dash")]
        [TestCase("number123")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void ValidUsername_IsAllowed(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeTrue();
        }

        [Test]
        [TestCase("username", "blog-username")]
        [TestCase("two words", "blog-two-words")]
        [TestCase("with-dash", "blog-with-dash")]
        [TestCase("number123", "blog-number123")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_IsConverted(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("user'name", "blog-username")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_WithAposthrope_ShouldRemoveApostrophe(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("UPPERCASE", "blog-uppercase")]
        [TestCase("MixEDcaSE", "blog-mixedcase")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_WithUpperCaseCharacters_ShouldLowerCase(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("trailingdash-", "blog-trailingdash")]
        [TestCase("trailingspace ", "blog-trailingspace")]
        [TestCase("-leadingdash", "blog-leadingdash")]
        [TestCase(" leadingspace", "blog-leadingspace")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_WithSpacesOrDashes_ShouldTrim(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("a")]
        [TestCase("ab")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void InvalidUsername_TooShort_IsNotAllowed(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeFalse();
        }

        [Test]
        [TestCase("a")]
        [TestCase("ab")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_TooShort_Throws(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("qwertyuiopasdfghjk")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void InvalidUsername_TooLong_IsNotAllowed(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeFalse();
        }

        [Test]
        [TestCase("qwertyuiopasdfghjk")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_TooLong_Throws(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("blog-something")]
        [TestCase("chat-something")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void InvalidUsername_StartsWithForbiddenWord_IsNotAllowed(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeFalse();
        }

        [Test]
        [TestCase("blog-something")]
        [TestCase("chat-something")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_StartsWithForbiddenWord_Throws(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("foobar")]
        [TestCase("abcfoobarabc")]
        [TestCase("abcfOOBarabc")]
        [TestCase("abc-foobar-abc")]
        [TestCase("abc-fOOBar-abc")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void InvalidUsername_WithForbiddenWord_IsNotAllowed(string username)
        {
            base.Fixture.Freeze<IOptionsMonitor<BlogsManagementOptions>>().CurrentValue.Returns(
                new BlogsManagementOptions() { ForbiddenChannelNameWords = new string[] { "foobar" } });
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeFalse();
        }

        [Test]
        [TestCase("foobar")]
        [TestCase("abcfoobarabc")]
        [TestCase("abcfOOBarabc")]
        [TestCase("abc-foobar-abc")]
        [TestCase("abc-fOOBar-abc")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_WithForbiddenWord_Throws(string username)
        {
            base.Fixture.Freeze<IOptionsMonitor<BlogsManagementOptions>>().CurrentValue.Returns(
                new BlogsManagementOptions() { ForbiddenChannelNameWords = new string[] { "foobar" } });
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }
    }
}
