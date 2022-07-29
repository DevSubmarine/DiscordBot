using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = DiscordInteractions.Test.CSharpCodeFixVerifier<
    DevSubmarine.Analyzers.DiscordInteractions.DiscordInteractionsAnalyzer,
    DevSubmarine.Analyzers.DiscordInteractions.NotDevSubBaseClassCodeFixProvider>;

namespace DiscordInteractions.Test
{
    [TestClass]
    public class DiscordInteractionsUnitTest
    {
    }
}
