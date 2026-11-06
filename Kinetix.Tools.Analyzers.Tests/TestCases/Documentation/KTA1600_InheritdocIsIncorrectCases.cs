namespace Kinetix.Tools.Analyzers.Tests.TestCases.Documentation;

public static class KTA1600_InheritdocIsIncorrectCases
{
    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            yield return Create("MissingInheritdoc", "");
            yield return Create("MissingCref", "/// <inheritdoc />");
            yield return Create("CrefTargetsInterface", "/// <inheritdoc cref=\"IExample\" />");
            yield return Create("CrefTargetsOtherMethod", "/// <inheritdoc cref=\"IExample.Other\" />");
            yield return Create("MissingOverloadInheritdoc", "", overloaded: true);
            yield return Create(
                "MissingOverloadParameterTypes",
                "/// <inheritdoc cref=\"IExample.Run\" />",
                overloaded: true
            );
            yield return Create(
                "MissingGenericTypeParameters",
                "/// <inheritdoc cref=\"IExample.Run\" />",
                generic: true
            );
        }
    }

    private static DiagnosticTestCase Create(
        string name,
        string documentation,
        bool overloaded = false,
        bool generic = false
    )
    {
        var contract =
            overloaded ? "void Run(int value); void Run(string text);"
            : generic ? "void Run<T>();"
            : "void Run();";
        var signature =
            overloaded ? "void Run(int value)"
            : generic ? "void Run<T>()"
            : "void Run()";
        var cref =
            overloaded ? "IExample.Run(int)"
            : generic ? "IExample.Run{T}"
            : "IExample.Run";
        const string otherOverload = """
            /// <inheritdoc cref="IExample.Run(string)" />
            public void Run(string text) {}
            """;
        string Build(string comment) =>
            $$"""
                interface IExample { {{contract}} }
                class Example : IExample
                {
                    {{comment}}
                    public {{signature}} {}
                    {{(overloaded ? otherOverload : "")}}
                }
                """;
        return new(name, Build(documentation), Build($"/// <inheritdoc cref=\"{cref}\" />"), "Run");
    }
}
