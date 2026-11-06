using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Naming;
using Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class RenameServiceParameterFixTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1501_ServiceCtrParameterNamingAnalyserCases.All),
        typeof(KTA1501_ServiceCtrParameterNamingAnalyserCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1501_ServiceCtrParameterNamingAnalyser(),
            new RenameServiceParameterFix()
        );
}
