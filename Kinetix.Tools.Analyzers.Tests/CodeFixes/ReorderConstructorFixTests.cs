using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Maintainability;
using Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class ReorderConstructorFixTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1201_ConstructorShouldBeOrderedCases.All), typeof(KTA1201_ConstructorShouldBeOrderedCases))]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1201_ConstructorShouldBeOrdered(),
            new ReorderConstructorFix()
        );
}
