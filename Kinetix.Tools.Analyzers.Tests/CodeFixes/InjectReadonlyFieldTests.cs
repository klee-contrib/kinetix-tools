using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class InjectReadonlyFieldTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1103_ReadonlyFieldsShouldBeInjectedCases.All),
        typeof(KTA1103_ReadonlyFieldsShouldBeInjectedCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1103_ReadonlyFieldsShouldBeInjected(),
            new InjectReadonlyField()
        );
}
