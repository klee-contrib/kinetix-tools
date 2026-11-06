using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Design;

[TestClass]
public sealed class KTA1103_ReadonlyFieldsShouldBeInjectedTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1103_ReadonlyFieldsShouldBeInjectedCases.All),
        typeof(KTA1103_ReadonlyFieldsShouldBeInjectedCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1103_ReadonlyFieldsShouldBeInjected());

    [TestMethod]
    [DataRow("class Example { private object _dependency; }")]
    [DataRow("class Example { private readonly object _dependency = new object(); }")]
    [DataRow(
        "class Example { private readonly object _dependency; public Example(object dependency) { _dependency = dependency; } }"
    )]
    [DataRow(
        "class Example { private readonly object _dependency; public Example(object dependency) { this._dependency = dependency; } }"
    )]
    [DataRow(
        "class Example { private static readonly object _dependency; static Example() { _dependency = new object(); } }"
    )]
    [DataRow(
        "class Example { private readonly int _dependency; public Example(string value) { int.TryParse(value, out _dependency); } }"
    )]
    [DataRow(
        "struct Example { private readonly int _dependency; public Example(int dependency) { _dependency = dependency; } }"
    )]
    public async Task DoesNotReportMutableOrInitializedFields(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1103_ReadonlyFieldsShouldBeInjected());

        Assert.IsEmpty(diagnostics);
    }
}
