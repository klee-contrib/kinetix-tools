using Kinetix.Tools.Analyzers.Diagnostics.Coverage;
using Kinetix.Tools.Analyzers.Tests.TestCases.Coverage;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Coverage;

[TestClass]
public sealed class KTA1300_DalMethodWithSqlServerCommandAnalyserTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1300_DalMethodWithSqlServerCommandAnalyserCases.All),
        typeof(KTA1300_DalMethodWithSqlServerCommandAnalyserCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1300_DalMethodWithSqlServerCommandAnalyser());

    [TestMethod]
    [DataRow("class DalExample", "GetSqlCommand")]
    [DataRow("[RegisterImpl] class ServiceExample", "GetBroker")]
    [DataRow("[RegisterImpl] class DalExample", "GetConnection")]
    public async Task DoesNotReportNonDalClassesOrOtherInvocations(string declaration, string invocation)
    {
        var source =
            ServiceTestSources.Annotations
            + $$"""
                {{declaration}}
                {
                    private object {{invocation}}() => null;
                    public object Load() => {{invocation}}();
                }
                """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new KTA1300_DalMethodWithSqlServerCommandAnalyser()
        );

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task DoesNotReportConstructorInvocations()
    {
        var source =
            ServiceTestSources.Annotations
            + """
                [RegisterImpl]
                class DalExample
                {
                    public DalExample() { GetSqlCommand(); }
                    private object GetSqlCommand() => null;
                }
                """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new KTA1300_DalMethodWithSqlServerCommandAnalyser()
        );

        Assert.IsEmpty(diagnostics);
    }
}
