using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class ReplaceImplementationWithContractFixTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1100_DoNotDependOnServiceImplementationAnalyzerCases.All),
        typeof(KTA1100_DoNotDependOnServiceImplementationAnalyzerCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1100_DoNotDependOnServiceImplementationAnalyzer(),
            new ReplaceImplementationWithContractFix()
        );
}
