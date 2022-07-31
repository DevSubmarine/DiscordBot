using AutoFixture.Kernel;
using DevSubmarine.DiscordBot.RandomStatus;
using DevSubmarine.DiscordBot.RandomStatus.Placeholders;
using DevSubmarine.DiscordBot.RandomStatus.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevSubmarine.DiscordBot.Tests.Features.RandomStatus
{
    [TestOf(typeof(StatusPlaceholderEngine))]
    public class StatusPlaceholderEngineTests : TestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            base.Fixture.Freeze<IServiceProvider>()
                .GetService(typeof(ILogger<StatusPlaceholderEngineTests>))
                .Returns(Substitute.For<ILogger<StatusPlaceholderEngineTests>>());
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingValidPlaceholder_WithoutDependencies_ReturnsTrue()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            bool result = engine.AddPlaceholder(typeof(ValidPlaceholder_NoDependencies));

            result.Should().BeTrue();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingValidPlaceholder_WithDependencies_ReturnsTrue()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            bool result = engine.AddPlaceholder(typeof(ValidPlaceholder_WithDependencies));

            result.Should().BeTrue();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingValidPlaceholder_DifferentPlaceholders_ReturnsTrue()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            bool result1 = engine.AddPlaceholder(typeof(ValidPlaceholder_NoDependencies));
            bool result2 = engine.AddPlaceholder(typeof(ValidPlaceholder_WithDependencies));

            result1.Should().BeTrue();
            result2.Should().BeTrue();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingValidPlaceholder_DuplicatedPlaceholders_ReturnsFalse()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            bool result1 = engine.AddPlaceholder(typeof(ValidPlaceholder_NoDependencies));
            bool result2 = engine.AddPlaceholder(typeof(ValidPlaceholder_NoDependencies));

            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingInvalidPlaceholder_MissingAttribute_Throws()
        {
            IStatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            Action act = () => engine.AddPlaceholder(typeof(InvalidPlaceholder_MissingAttribute));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingInvalidPlaceholder_MissingInterface_Throws()
        {
            IStatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            Action act = () => engine.AddPlaceholder(typeof(InvalidPlaceholder_MissingInterface));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingInvalidPlaceholder_Abstract_Throws()
        {
            IStatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            Action act = () => engine.AddPlaceholder(typeof(InvalidPlaceholder_Abstract));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngine.AddPlaceholder))]
        public void AddingInvalidPlaceholder_Generic_Throws()
        {
            IStatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();

            Action act = () => engine.AddPlaceholder(typeof(InvalidPlaceholder_Generic<>));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngineExtensions.AddPlaceholders))]
        [Category(TestCategoryName.Extensions)]
        public void AddingMultipleValidPlaceholders_NoDuplication_ReturnsCount()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            Type[] types = { typeof(ValidPlaceholder_NoDependencies), typeof(ValidPlaceholder_WithDependencies) };

            int result = engine.AddPlaceholders(types);

            result.Should().Be(2);
        }

        [Test]
        [Category(nameof(StatusPlaceholderEngineExtensions.AddPlaceholders))]
        [Category(TestCategoryName.Extensions)]
        public void AddingMultipleValidPlaceholders_WithDuplication_ReturnsCount()
        {
            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            Type[] types = { typeof(ValidPlaceholder_NoDependencies), typeof(ValidPlaceholder_NoDependencies) };

            int result = engine.AddPlaceholders(types);

            result.Should().Be(1);
        }

        [Test]
        [Repeat(3)]
        [Category(nameof(StatusPlaceholderEngine.ConvertPlaceholdersAsync))]
        public async Task ConvertingStatus_WithPlaceholder_ReplacesCorrectly()
        {
            Type placeholderType = typeof(ValidPlaceholder_NoDependencies);
            string placeholder = $"{{{placeholderType.Name}}}";

            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            engine.AddPlaceholder(placeholderType);

            string result = await engine.ConvertPlaceholdersAsync(placeholder);

            result.Should().Be(placeholderType.Name);
        }
        [Test]
        [Repeat(3)]
        [Category(nameof(StatusPlaceholderEngine.ConvertPlaceholdersAsync))]
        public async Task ConvertingStatus_WithNoise_WithSamePlaceholders_ReplacesCorrectly()
        {
            Type placeholderType = typeof(ValidPlaceholder_NoDependencies);
            string placeholder = $"{{{placeholderType.Name}}}";
            string noise1 = base.Fixture.Create<string>() + "abc";
            string noise2 = base.Fixture.Create<string>() + "d";
            string noise3 = base.Fixture.Create<string>();

            string status = $"{noise1} {placeholder} {noise2} {placeholder} {noise3}";
            string expectedResult = $"{noise1} {placeholderType.Name} {noise2} {placeholderType.Name} {noise3}";

            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            engine.AddPlaceholder(placeholderType);

            string result = await engine.ConvertPlaceholdersAsync(status);

            result.Should().Be(expectedResult);
        }

        [Test]
        [Repeat(3)]
        [Category(nameof(StatusPlaceholderEngine.ConvertPlaceholdersAsync))]
        public async Task ConvertingStatus_WithNoise_WithDifferentPlaceholders_ReplacesCorrectly()
        {
            Type placeholderType1 = typeof(ValidPlaceholder_NoDependencies);
            Type placeholderType2 = typeof(ValidPlaceholder_WithDependencies);
            string placeholder1 = $"{{{placeholderType1.Name}}}";
            string placeholder2 = $"{{{placeholderType2.Name}}}";
            string noise1 = base.Fixture.Create<string>() + "abc";
            string noise2 = base.Fixture.Create<string>() + "d";
            string noise3 = base.Fixture.Create<string>();

            string status = $"{noise1} {placeholder1} {noise2} {placeholder2} {noise3}";
            string expectedResult = $"{noise1} {placeholderType1.Name} {noise2} {placeholderType2.Name} {noise3}";

            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            engine.AddPlaceholder(placeholderType1);
            engine.AddPlaceholder(placeholderType2);

            string result = await engine.ConvertPlaceholdersAsync(status);

            result.Should().Be(expectedResult);
        }

        [Test]
        [Repeat(3)]
        [Category(nameof(StatusPlaceholderEngine.ConvertPlaceholdersAsync))]
        public async Task ConvertingStatus_WithoutPlaceholders_NotChangingStatus()
        {
            string status = base.Fixture.Create<string>();

            StatusPlaceholderEngine engine = base.Fixture.Create<StatusPlaceholderEngine>();
            engine.AddPlaceholder(typeof(ValidPlaceholder_NoDependencies));
            engine.AddPlaceholder(typeof(ValidPlaceholder_WithDependencies));

            string result = await engine.ConvertPlaceholdersAsync(status);

            result.Should().Be(status);
        }

        #region Test Placeholder Types
        // VALID
        [StatusPlaceholder($"{{{nameof(ValidPlaceholder_NoDependencies)}}}")]
        private class ValidPlaceholder_NoDependencies : IStatusPlaceholder
        {
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }

        [StatusPlaceholder($"{{{nameof(ValidPlaceholder_WithDependencies)}}}")]
        public class ValidPlaceholder_WithDependencies : IStatusPlaceholder
        {
            public ValidPlaceholder_WithDependencies(ILogger<StatusPlaceholderEngineTests> log) { }
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }

        // INVALID
        private class InvalidPlaceholder_MissingAttribute : IStatusPlaceholder
        {
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }

        [StatusPlaceholder($"{{{nameof(InvalidPlaceholder_MissingInterface)}}}")]
        private class InvalidPlaceholder_MissingInterface
        {
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }

        [StatusPlaceholder($"{{{nameof(InvalidPlaceholder_Abstract)}}}")]
        private abstract class InvalidPlaceholder_Abstract : IStatusPlaceholder
        {
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }

        [StatusPlaceholder($"{{InvalidPlaceholder_Generic}}")]
        private class InvalidPlaceholder_Generic<T> : IStatusPlaceholder
        {
            public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(this.GetType().Name);
        }
        #endregion
    }
}
