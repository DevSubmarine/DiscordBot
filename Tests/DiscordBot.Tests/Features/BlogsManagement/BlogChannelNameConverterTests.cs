using DevSubmarine.DiscordBot.BlogsManagement;
using DevSubmarine.DiscordBot.BlogsManagement.Services;

namespace DevSubmarine.DiscordBot.Tests.Features.BlogsManagement
{
    [TestOf(typeof(BlogChannelNameConverter))]
    public class BlogChannelNameConverterTests : TestBase
    {
        [Test]
        [TestCase("username")]
        [TestCase("with.dot")]
        [TestCase("with_underscore")]
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
        [TestCase("with.dot", "blog-with-dot")]
        [TestCase("with_underscore", "blog-with-underscore")]
        [TestCase("number123", "blog-number123")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_IsConverted(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("username", "blog-username")]
        [TestCase("with.dot", "blog-with-dot")]
        [TestCase("with_underscore", "blog-with-underscore")]
        [TestCase("number123", "blog-number123")]
        [Category(nameof(BlogChannelNameConverterExtensions.TryConvertUsername))]
        public void ValidUsername_IsAllowedAndConverts(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool success = service.TryConvertUsername(username, out string result);

            success.Should().BeTrue();
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
        [TestCase("UPPERCASE", "blog-uppercase")]
        [TestCase("MixEDcaSE", "blog-mixedcase")]
        [Category(nameof(BlogChannelNameConverterExtensions.TryConvertUsername))]
        public void ValidUsername_WithUpperCaseCharacters_ShouldSucceedAndLowerCase(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool success = service.TryConvertUsername(username, out string result);

            success.Should().BeTrue();
            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("trailingdot.", "blog-trailingdot")]
        [TestCase("trailingunderscore_", "blog-trailingunderscore")]
        [TestCase(".leadingdot", "blog-leadingdot")]
        [TestCase("_leadingunderscore", "blog-leadingunderscore")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void ValidUsername_WithSpacesUnderscoresOrDashes_ShouldTrim(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            string result = service.ConvertUsername(username);

            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("trailingdot.", "blog-trailingdot")]
        [TestCase("trailingunderscore_", "blog-trailingunderscore")]
        [TestCase(".leadingdot", "blog-leadingdot")]
        [TestCase("_leadingunderscore", "blog-leadingunderscore")]
        [Category(nameof(BlogChannelNameConverterExtensions.TryConvertUsername))]
        public void ValidUsername_WithSpacesUnderscoresOrDashes_ShouldSucceedAndTrim(string username, string expectedResult)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool success = service.TryConvertUsername(username, out string result);

            success.Should().BeTrue();
            result.Should().Be(expectedResult);
        }

        [Test]
        [TestCase("blog_something")]
        [TestCase("chat_something")]
        [Category(nameof(BlogChannelNameConverter.IsUsernameAllowed))]
        public void InvalidUsername_StartsWithForbiddenWord_IsNotAllowed(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool result = service.IsUsernameAllowed(username);

            result.Should().BeFalse();
        }

        [Test]
        [TestCase("blog_something")]
        [TestCase("chat_something")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_StartsWithForbiddenWord_Throws(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("blog_something")]
        [TestCase("chat_something")]
        [Category(nameof(BlogChannelNameConverterExtensions.TryConvertUsername))]
        public void InvalidUsername_StartsWithForbiddenWord_ShouldFail(string username)
        {
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool success = service.TryConvertUsername(username, out string result);

            success.Should().BeFalse();
        }

        [Test]
        [TestCase("foobar")]
        [TestCase("abcfoobarabc")]
        [TestCase("abcfOOBarabc")]
        [TestCase("abc_foobar_abc")]
        [TestCase("abc_fOOBar_abc")]
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
        [TestCase("abc_foobar_abc")]
        [TestCase("abc_fOOBar_abc")]
        [Category(nameof(BlogChannelNameConverter.ConvertUsername))]
        public void InvalidUsername_WithForbiddenWord_Throws(string username)
        {
            base.Fixture.Freeze<IOptionsMonitor<BlogsManagementOptions>>().CurrentValue.Returns(
                new BlogsManagementOptions() { ForbiddenChannelNameWords = new string[] { "foobar" } });
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            Action act = () => service.ConvertUsername(username);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        [TestCase("foobar")]
        [TestCase("abcfoobarabc")]
        [TestCase("abcfOOBarabc")]
        [TestCase("abc_foobar_abc")]
        [TestCase("abc_fOOBar_abc")]
        [Category(nameof(BlogChannelNameConverterExtensions.TryConvertUsername))]
        public void InvalidUsername_WithForbiddenWord_ShouldFail(string username)
        {
            base.Fixture.Freeze<IOptionsMonitor<BlogsManagementOptions>>().CurrentValue.Returns(
                new BlogsManagementOptions() { ForbiddenChannelNameWords = new string[] { "foobar" } });
            BlogChannelNameConverter service = base.Fixture.Create<BlogChannelNameConverter>();

            bool success = service.TryConvertUsername(username, out string result);

            success.Should().BeFalse();
        }
    }
}
