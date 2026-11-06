using Kinetix.Tools.Analyzers.Diagnostics.Design;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Design;

[TestClass]
public sealed class KTA1101_DoNotUseBusinessImplementationAnalyzerTests
{
    private const string DependencySource = "namespace External { public class BusinessObject { } }";

    [TestMethod]
    [DataRow("using External; class Example { private BusinessObject _dependency; }")]
    [DataRow("using External; class Example { public Example(BusinessObject dependency) {} }")]
    [DataRow("using External; class Example : BusinessObject { }")]
    [DataRow("using External; class Example { public BusinessObject Create() => null; }")]
    [DataRow("using External; class Example { public object Create() => new BusinessObject(); }")]
    public async Task ReportsReferencesToAnotherBusinessImplementation(string source)
    {
        var reference = AnalyzerTestHelper.CreateReference(DependencySource, "CatalogImplementation");
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new KTA1101_DoNotUseBusinessImplementationAnalyzer(),
            "OrdersImplementation",
            reference
        );

        AnalyzerTestHelper.AssertDiagnostic(diagnostics, "KTA1101", "BusinessObject");
        StringAssert.Contains(diagnostics[0].GetMessage(), "BusinessObject");
    }

    [TestMethod]
    [DataRow("OrdersImplementation", "CatalogContracts")]
    [DataRow("OrdersApplication", "CatalogImplementation")]
    [DataRow("OrdersApplication", "CatalogContracts")]
    public async Task DoesNotReportWhenEitherAssemblyIsNotAnImplementation(string assemblyName, string dependencyName)
    {
        var reference = AnalyzerTestHelper.CreateReference(DependencySource, dependencyName);
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            "using External; class Example { private BusinessObject _dependency; }",
            new KTA1101_DoNotUseBusinessImplementationAnalyzer(),
            assemblyName,
            reference
        );

        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task DoesNotReportReferencesWithinTheSameImplementation()
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            DependencySource + "class Example { private External.BusinessObject _dependency; }",
            new KTA1101_DoNotUseBusinessImplementationAnalyzer(),
            "OrdersImplementation"
        );

        Assert.IsEmpty(diagnostics);
    }
}
