using Kinetix.Tools.Analyzers.Diagnostics.Design;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Design;

[TestClass]
public sealed class KTA1102_ServiceImplementationPublicMethodAnalyserTests
{
    private const string Contract =
        ServiceTestSources.Annotations
        + """
            [RegisterContract]
            interface IServiceExample { void Run(); }

            """;

    [TestMethod]
    [DataRow("public void Extra() {}", "Extra")]
    [DataRow("public void Run(int value) {}", "Run")]
    [DataRow("public void Run(string value) {}", "Run")]
    [DataRow("public static void Extra() {}", "Extra")]
    public async Task ReportsPublicMethodsNotPublishedInTheContract(string method, string methodName)
    {
        var source =
            Contract
            + $$"""
                [RegisterImpl]
                class Example : IServiceExample
                {
                    public void Run() {}
                    {{method}}
                }
                """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new KTA1102_ServiceImplementationPublicMethodAnalyser()
        );

        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "KTA1102", methodName);
        StringAssert.Contains(diagnostics[0].GetMessage(), methodName);
        StringAssert.Contains(diagnostics[0].GetMessage(), "Example");
    }

    [TestMethod]
    [DataRow("[RegisterImpl] class Example : IServiceExample { public void Run() {} }")]
    [DataRow("[RegisterImpl] class Example : IServiceExample { public void Run() {} private void Extra() {} }")]
    [DataRow("[RegisterImpl] class Example : IServiceExample { public void Run() {} protected void Extra() {} }")]
    [DataRow("[RegisterImpl] class Example : IServiceExample { void IServiceExample.Run() {} }")]
    [DataRow("class Example : IServiceExample { public void Run() {} public void Extra() {} }")]
    [DataRow("[RegisterImpl] class Example { public void Extra() {} }")]
    public async Task DoesNotReportPublishedOrNonPublicMethodsOrNonServices(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            Contract + source,
            new KTA1102_ServiceImplementationPublicMethodAnalyser()
        );

        Assert.IsEmpty(diagnostics);
    }
}
