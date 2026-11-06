using Kinetix.Tools.Analyzers.Diagnostics.Documentation;
using Kinetix.Tools.Analyzers.Tests.TestCases.Documentation;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Documentation;

[TestClass]
public sealed class KTA1600_InheritdocIsIncorrectTests
{
    [TestMethod]
    [DynamicData(nameof(KTA1600_InheritdocIsIncorrectCases.All), typeof(KTA1600_InheritdocIsIncorrectCases))]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1600_InheritdocIsIncorrect());

    [TestMethod]
    [DataRow("/// <inheritdoc cref=\"IExample.Run\" />")]
    [DataRow("/// <summary>Runs the operation.</summary>")]
    public async Task DoesNotReportCorrectInheritdocOrExplicitDocumentation(string documentation)
    {
        var source = $$"""
            interface IExample { void Run(); }
            class Example : IExample
            {
                {{documentation}}
                public void Run() {}
            }
            """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1600_InheritdocIsIncorrect());

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    [DataRow("class Example { public void Run() {} }")]
    [DataRow("interface IExample { void Run(); }")]
    [DataRow(
        "interface IExample { void Run(); } [System.CodeDom.Compiler.GeneratedCode(\"Test\", \"1.0\")] class Example : IExample { public void Run() {} }"
    )]
    public async Task DoesNotReportMethodsOutsideItsScope(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1600_InheritdocIsIncorrect());

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task DoesNotReportCorrectOverloadParameterTypes()
    {
        const string source = """
            interface IExample { void Run(int value); void Run(string text); }
            class Example : IExample
            {
                /// <inheritdoc cref="IExample.Run(int)" />
                public void Run(int value) {}
                /// <inheritdoc cref="IExample.Run(string)" />
                public void Run(string text) {}
            }
            """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1600_InheritdocIsIncorrect());

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task DoesNotReportCorrectGenericTypeParameters()
    {
        const string source = """
            interface IExample { void Run<T>(); }
            class Example : IExample
            {
                /// <inheritdoc cref="IExample.Run{T}" />
                public void Run<T>() {}
            }
            """;
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1600_InheritdocIsIncorrect());

        Assert.IsEmpty(diagnostics);
    }
}
