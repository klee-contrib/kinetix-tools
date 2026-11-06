using Kinetix.Tools.Analyzers.Diagnostics.Maintainability;
using Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Maintainability;

[TestClass]
public sealed class KTA1201_ConstructorShouldBeOrderedTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1201_ConstructorShouldBeOrderedCases.All), typeof(KTA1201_ConstructorShouldBeOrderedCases))]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1201_ConstructorShouldBeOrdered());

    [TestMethod]
    [DataRow("class Example { private int _a; public Example(int a) { _a = a; } }")]
    [DataRow(
        "class Example { private int _a; private int _z; public Example(int a, int z) { if (a < 0) throw new System.ArgumentOutOfRangeException(nameof(a)); _a = a; _z = z; System.Console.WriteLine(a); } }"
    )]
    [DataRow("class Example { public Example() { System.Console.WriteLine(1); } }")]
    [DataRow("class Example { private int _a; public void Run(int a) { System.Console.WriteLine(a); _a = a; } }")]
    public async Task DoesNotReportWhenConstructorStatementsAreOrderedOrAbsent(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1201_ConstructorShouldBeOrdered());

        Assert.IsEmpty(diagnostics);
    }
}
