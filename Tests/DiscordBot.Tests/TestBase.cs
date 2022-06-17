global using NUnit.Framework;
global using AutoFixture;
global using AutoFixture.AutoNSubstitute;
global using FluentAssertions;
global using NSubstitute;

global using System;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using Microsoft.Extensions.Options;

namespace DevSubmarine.DiscordBot.Tests
{
    public abstract class TestBase
    {
        protected IFixture Fixture { get; set; }

        [SetUp]
        public virtual void SetUp()
        {
            this.Fixture = new Fixture()
                .Customize(new AutoNSubstituteCustomization()
                {
                    ConfigureMembers = true,
                    GenerateDelegates = true
                });
        }
    }
}
