using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Design;

[TestClass]
public sealed class KTA1104_AsyncMethodShouldHaveCancellationTokenTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1104_AsyncMethodShouldHaveCancellationTokenCases.All),
        typeof(KTA1104_AsyncMethodShouldHaveCancellationTokenCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1104_AsyncMethodShouldHaveCancellationToken());

    [TestMethod]
    [DataRow("Task RunAsync(CancellationToken cancellationToken) { CancellableAsync(); return Task.CompletedTask; }")]
    [DataRow("Task RunAsync() { CancellableAsync(CancellationToken.None); return Task.CompletedTask; }")]
    [DataRow("Task RunAsync() { WithParamsAsync(CancellationToken.None); return Task.CompletedTask; }")]
    [DataRow("Task RunAsync() { WithoutTokenAsync(); return Task.CompletedTask; }")]
    [DataRow("Task RunAsync() { DifferentOverloadAsync(1); return Task.CompletedTask; }")]
    [DataRow("int Run() { CancellableAsync(); return 1; }")]
    public async Task DoesNotReportWhenCancellationIsNotApplicable(string method)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            KTA1104_AsyncMethodShouldHaveCancellationTokenCases.BuildSource(method),
            new KTA1104_AsyncMethodShouldHaveCancellationToken()
        );

        Assert.IsEmpty(diagnostics);
    }
}
