using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Naming;
using Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class RenameServiceFieldFixTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1500_ServiceFieldNamingAnalyserCases.All), typeof(KTA1500_ServiceFieldNamingAnalyserCases))]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1500_ServiceFieldNamingAnalyser(),
            new RenameServiceFieldFix()
        );
}
