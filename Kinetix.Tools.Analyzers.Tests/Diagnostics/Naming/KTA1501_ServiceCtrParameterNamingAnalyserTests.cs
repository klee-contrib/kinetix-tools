using Kinetix.Tools.Analyzers.Diagnostics.Naming;
using Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Naming;

[TestClass]
public sealed class KTA1501_ServiceCtrParameterNamingAnalyserTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1501_ServiceCtrParameterNamingAnalyserCases.All),
        typeof(KTA1501_ServiceCtrParameterNamingAnalyserCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1501_ServiceCtrParameterNamingAnalyser());

    [TestMethod]
    [DataRow(
        "[RegisterContract] interface IServiceExample {} class Example { public Example(IServiceExample serviceExample) {} }"
    )]
    [DataRow("[RegisterContract] interface IDalExample {} class Example { public Example(IDalExample dalExample) {} }")]
    [DataRow("interface IServiceExample {} class Example { public Example(IServiceExample dependency) {} }")]
    [DataRow("[RegisterContract] interface IRepository {} class Example { public Example(IRepository dependency) {} }")]
    [DataRow("class Example { public Example(object dependency) {} }")]
    [DataRow(
        "[RegisterContract] interface IServiceExample {} class Example { public void Run(IServiceExample dependency) {} }"
    )]
    public async Task DoesNotReportCorrectNamesOrNonServiceConstructorParameters(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            ServiceTestSources.Annotations + source,
            new KTA1501_ServiceCtrParameterNamingAnalyser()
        );

        Assert.IsEmpty(diagnostics);
    }
}
