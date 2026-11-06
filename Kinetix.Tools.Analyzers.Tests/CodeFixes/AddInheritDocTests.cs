using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Documentation;
using Kinetix.Tools.Analyzers.Tests.TestCases.Documentation;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class AddInheritDocTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1600_InheritdocIsIncorrectCases.All), typeof(KTA1600_InheritdocIsIncorrectCases))]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(testCase, new KTA1600_InheritdocIsIncorrect(), new AddInheritDoc());
}
