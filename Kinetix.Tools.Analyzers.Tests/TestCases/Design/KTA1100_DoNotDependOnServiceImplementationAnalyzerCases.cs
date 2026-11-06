namespace Kinetix.Tools.Analyzers.Tests.TestCases.Design;

public static class KTA1100_DoNotDependOnServiceImplementationAnalyzerCases
{
    private const string Services =
        ServiceTestSources.Annotations
        + """
            namespace Services
            {
                [RegisterContract] public interface IServiceExample {}
                [RegisterImpl] public class ServiceExample : IServiceExample {}
            }
            """;

    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            foreach (
                var (name, signature) in new[]
                {
                    ("PublicConstructor", "public Example(ServiceExample dependency)"),
                    ("PrivateConstructor", "private Example(ServiceExample dependency)"),
                    ("MultipleParameters", "public Example(int value, ServiceExample dependency)"),
                }
            )
            {
                yield return new(
                    name,
                    ServiceTestSources.Service + "class Example { " + signature + " {} }",
                    ServiceTestSources.Service
                        + "class Example { "
                        + signature.Replace("ServiceExample dependency", "IServiceExample dependency")
                        + " {} }",
                    "ServiceExample"
                )
                {
                    MessageContains = ["Example", "ServiceExample"],
                };
            }

            foreach (
                var (name, source, location) in new[]
                {
                    (
                        "AddImport",
                        "class Example { public Example(Services.ServiceExample dependency) {} }",
                        "Services.ServiceExample"
                    ),
                    (
                        "ExistingImport",
                        "using Services; class Example { public Example(ServiceExample dependency) {} }",
                        "ServiceExample"
                    ),
                }
            )
            {
                yield return new(
                    name,
                    source,
                    "using Services; class Example { public Example(IServiceExample dependency) {} }",
                    location
                )
                {
                    AdditionalDocuments = [new("Services.cs", Services, Services)],
                    MessageContains = ["Example", "ServiceExample"],
                };
            }

            const string multipleContracts =
                ServiceTestSources.Annotations
                + """
                    namespace Services
                    {
                        [RegisterContract] public interface IServiceFirst {}
                        [RegisterContract] public interface IServiceSecond {}
                        [RegisterImpl] public class ServiceExample : IServiceFirst, IServiceSecond {}
                    }
                    """;
            foreach (var (actionIndex, contractName) in new[] { (0, "IServiceFirst"), (1, "IServiceSecond") })
            {
                yield return new(
                    "Select_" + contractName,
                    "class Example { public Example(Services.ServiceExample dependency) {} }",
                    $"using Services; class Example {{ public Example({contractName} dependency) {{}} }}",
                    "Services.ServiceExample"
                )
                {
                    AdditionalDocuments = [new("Services.cs", multipleContracts, multipleContracts)],
                    ActionCount = 2,
                    ActionIndex = actionIndex,
                    MessageContains = ["Example", "ServiceExample"],
                };
            }
        }
    }
}
