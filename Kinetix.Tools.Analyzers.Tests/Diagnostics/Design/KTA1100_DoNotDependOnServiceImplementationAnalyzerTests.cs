using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Design;

[TestClass]
public sealed class KTA1100_DoNotDependOnServiceImplementationAnalyzerTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1100_DoNotDependOnServiceImplementationAnalyzerCases.All),
        typeof(KTA1100_DoNotDependOnServiceImplementationAnalyzerCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(
            testCase,
            new KTA1100_DoNotDependOnServiceImplementationAnalyzer()
        );

    [TestMethod]
    [DataRow("class Example { public Example(IServiceExample serviceExample) { } }")]
    [DataRow("class Example { public Example(object dependency) { } }")]
    [DataRow("class Example { public void Run(ServiceExample dependency) { } }")]
    [DataRow("struct Example { public Example(ServiceExample dependency) { } }")]
    [DataRow(
        "class Unregistered : IServiceExample { public void Run() {} } class Example { public Example(Unregistered dependency) {} }"
    )]
    [DataRow("[RegisterImpl] class NoContract {} class Example { public Example(NoContract dependency) {} }")]
    public async Task DoesNotReportForContractsOrNonServiceDependencies(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            ServiceTestSources.Service + source,
            new KTA1100_DoNotDependOnServiceImplementationAnalyzer()
        );

        Assert.IsEmpty(diagnostics);
    }
}
