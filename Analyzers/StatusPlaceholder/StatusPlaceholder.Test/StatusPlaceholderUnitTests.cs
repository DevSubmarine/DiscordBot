using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = DevSubmarine.Analyzers.StatusPlaceholder.Tests.CSharpCodeFixVerifier<
    DevSubmarine.Analyzers.StatusPlaceholder.StatusPlaceholderAnalyzer,
    DevSubmarine.Analyzers.StatusPlaceholder.MissingInterfaceCodeFixProvider>;

namespace DevSubmarine.Analyzers.StatusPlaceholder.Tests
{
    [TestClass]
    public class StatusPlaceholderUnitTest
    {
    }
}
