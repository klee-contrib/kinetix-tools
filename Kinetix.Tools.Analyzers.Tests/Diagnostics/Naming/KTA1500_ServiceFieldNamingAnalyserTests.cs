using Kinetix.Tools.Analyzers.Diagnostics.Naming;
using Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Naming;

[TestClass]
public sealed class KTA1500_ServiceFieldNamingAnalyserTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1500_ServiceFieldNamingAnalyserCases.All), typeof(KTA1500_ServiceFieldNamingAnalyserCases))]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1500_ServiceFieldNamingAnalyser());

    [TestMethod]
    [DataRow(
        "[RegisterContract] interface IServiceExample {} class Example { private IServiceExample _serviceExample; }"
    )]
    [DataRow("[RegisterContract] interface IDalExample {} class Example { private IDalExample _dalExample; }")]
    [DataRow("interface IServiceExample {} class Example { private IServiceExample _dependency; }")]
    [DataRow("[RegisterContract] interface IRepository {} class Example { private IRepository _dependency; }")]
    [DataRow("class Example { private object _dependency; }")]
    [DataRow(
        "[RegisterContract] interface IServiceExample {} class Example { public IServiceExample Dependency { get; set; } }"
    )]
    public async Task DoesNotReportCorrectNamesOrNonServiceFields(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            ServiceTestSources.Annotations + source,
            new KTA1500_ServiceFieldNamingAnalyser()
        );

        Assert.IsEmpty(diagnostics);
    }
}
