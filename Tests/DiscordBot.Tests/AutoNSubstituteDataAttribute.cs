using AutoFixture.NUnit3;

namespace AutoFixture
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()
            {
                ConfigureMembers = true,
                GenerateDelegates = true
            }))
        {
        }
    }
}
