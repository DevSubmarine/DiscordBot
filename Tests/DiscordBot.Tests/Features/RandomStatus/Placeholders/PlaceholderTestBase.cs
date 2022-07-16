using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using NUnit.Framework.Internal;
using System.Reflection;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus.Placeholders
{
    public abstract class PlaceholderTestBase : TestBase
    {
        protected abstract Type PlaceholderType { get; }

        protected StatusPlaceholderAttribute PlaceholderAttribute { get; set; }

        protected string PlaceholderText => this.PlaceholderAttribute.Placeholder;
        protected Regex PlaceholderRegex => this.PlaceholderAttribute.PlaceholderRegex;


        protected Match CreateTestMatch(string input)
            => this.PlaceholderRegex.Match(input);
        protected Match CreateTestMatch()
            => this.CreateTestMatch($"{base.Fixture.Create<string>()} {this.PlaceholderText} {base.Fixture.Create<string>()}");


        public override void SetUp()
        {
            base.SetUp();

            if (this.PlaceholderType == null)
                throw new InvalidOperationException($"Status placeholder tests need to have {nameof(PlaceholderType)} property set.");

            this.PlaceholderAttribute = this.PlaceholderType.GetCustomAttribute<StatusPlaceholderAttribute>();
        }

        [Test]
        [Category(TestCategoryName.Interfaces)]
        public void StatusPlaceholders_ShouldImplementInterface()
        {
            this.PlaceholderType.Should().Implement<IStatusPlaceholder>();
        }

        [Test]
        [Category(TestCategoryName.Attributes)]
        public void StatusPlaceholders_ShouldBeDecoratedWithAttribute()
        {
            this.PlaceholderAttribute.Should().NotBeNull();
            this.PlaceholderAttribute.Placeholder.Should().NotBeNull();
            this.PlaceholderAttribute.PlaceholderRegex.Should().NotBeNull();
        }
    }
}
