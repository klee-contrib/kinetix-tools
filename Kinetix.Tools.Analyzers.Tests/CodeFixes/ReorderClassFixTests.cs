using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Maintainability;
using Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class ReorderClassFixTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1200_ClassMembersShouldBeOrderedCases.All),
        typeof(KTA1200_ClassMembersShouldBeOrderedCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1200_ClassMembersShouldBeOrdered(),
            new ReorderClassFix()
        );
}
