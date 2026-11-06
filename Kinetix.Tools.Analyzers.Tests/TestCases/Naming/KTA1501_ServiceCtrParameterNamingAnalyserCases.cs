namespace Kinetix.Tools.Analyzers.Tests.TestCases.Naming;

public static class KTA1501_ServiceCtrParameterNamingAnalyserCases
{
    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            yield return Create("ServiceParameter", "IServiceExample", "dependency", "serviceExample");
            yield return Create("UnexpectedUnderscore", "IServiceExample", "_serviceExample", "serviceExample");
            yield return Create("DalParameter", "IDalExample", "dependency", "dalExample");
            yield return Create("ServiceNamedArguments", "IServiceExample", "dependency", "serviceExample", true);
            yield return Create("DalNamedArguments", "IDalExample", "dependency", "dalExample", true);
        }
    }

    private static DiagnosticTestCase Create(
        string name,
        string contractName,
        string parameterName,
        string fixedName,
        bool separateDocument = false
    )
    {
        var prefix = ServiceTestSources.Annotations + $"[RegisterContract] interface {contractName} {{}}";
        var body = separateDocument ? $"System.Console.WriteLine({parameterName});" : "";
        var fixedBody = separateDocument ? $"System.Console.WriteLine({fixedName});" : "";
        return new(
            name,
            prefix + $"class Example {{ public Example({contractName} {parameterName}) {{ {body} }} }}",
            prefix + $"class Example {{ public Example({contractName} {fixedName}) {{ {fixedBody} }} }}",
            parameterName
        )
        {
            MessageContains = ["Example", fixedName],
            AdditionalDocuments = separateDocument
                ?
                [
                    new(
                        "Usage.cs",
                        "class Caller { object Create() => new Example(dependency: null); }",
                        $"class Caller {{ object Create() => new Example({fixedName}: null); }}"
                    ),
                ]
                : [],
        };
    }
}
