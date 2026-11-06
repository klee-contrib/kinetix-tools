namespace Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

public static class KTA1500_ServiceFieldNamingAnalyserCases
{
    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            yield return Create("ServiceField", "IServiceExample", "_dependency", "_serviceExample");
            yield return Create("MissingUnderscore", "IServiceExample", "serviceExample", "_serviceExample");
            yield return Create("DalField", "IDalExample", "_dependency", "_dalExample");
            yield return Create(
                "ServiceReferencesAcrossDocuments",
                "IServiceExample",
                "_dependency",
                "_serviceExample",
                true
            );
            yield return Create("DalReferencesAcrossDocuments", "IDalExample", "_dependency", "_dalExample", true);
        }
    }

    private static DiagnosticTestCase Create(
        string name,
        string contractName,
        string fieldName,
        string fixedName,
        bool separateDocument = false
    )
    {
        var prefix = ServiceTestSources.Annotations + $"[RegisterContract] interface {contractName} {{}}";
        var typeDeclaration = separateDocument ? "partial class" : "class";
        return new(
            name,
            prefix + $"{typeDeclaration} Example {{ private {contractName} {fieldName}; }}",
            prefix + $"{typeDeclaration} Example {{ private {contractName} {fixedName}; }}",
            fieldName
        )
        {
            MessageContains = ["Example", fixedName],
            AdditionalDocuments = separateDocument
                ?
                [
                    new(
                        "Usage.cs",
                        "partial class Example { public object Get() => _dependency; }",
                        $"partial class Example {{ public object Get() => {fixedName}; }}"
                    ),
                ]
                : [],
        };
    }
}
