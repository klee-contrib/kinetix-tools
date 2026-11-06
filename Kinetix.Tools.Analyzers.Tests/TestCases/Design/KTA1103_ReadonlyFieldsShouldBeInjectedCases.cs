namespace Kinetix.Tools.Analyzers.Tests.TestCases.Design;

public static class KTA1103_ReadonlyFieldsShouldBeInjectedCases
{
    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            foreach (
                var (name, source) in new[]
                {
                    ("NoConstructor", "class Example { private readonly object _dependency; }"),
                    ("StaticFieldWithoutConstructor", "class Example { private static readonly object _dependency; }"),
                    ("RecordWithoutConstructor", "record Example { private readonly object _dependency; }"),
                }
            )
            {
                // The current provider leaves fields unchanged when there is no constructor to edit.
                yield return new(name, source, source, "_dependency") { DiagnosticRemains = true };
            }

            yield return new(
                "ReuseExistingConstructorParameter",
                "class Example { private readonly object _dependency; public Example(object dependency) {} }",
                "class Example { private readonly object _dependency; public Example(object dependency) { _dependency = dependency; } }",
                "_dependency"
            );
            yield return new(
                "AddConstructorParameter",
                "class Example { private readonly object _dependency; public Example() {} }",
                "class Example { private readonly object _dependency; public Example(object dependency) { _dependency = dependency; } }",
                "_dependency"
            );
            yield return new(
                "AppendConstructorParameter",
                "class Example { private readonly int _dependency; public Example(string name) {} }",
                "class Example { private readonly int _dependency; public Example(string name, int dependency) { _dependency = dependency; } }",
                "_dependency"
            );
            yield return new(
                "ConflictingParameterType",
                "class Example { private readonly object _dependency; public Example(int dependency) {} }",
                "class Example { private readonly object _dependency; public Example(int dependency, object dependency1) { _dependency = dependency1; } }",
                "_dependency"
            );
            yield return new(
                "ReuseAliasedParameterType",
                "using Dependency = object; class Example { private readonly object _dependency; public Example(Dependency dependency) {} }",
                "using Dependency = object; class Example { private readonly object _dependency; public Example(Dependency dependency) { _dependency = dependency; } }",
                "_dependency"
            );
            yield return new(
                "PreserveExistingParameterDocumentation",
                """
                class Example
                {
                    private readonly object _dependency;
                    /// <summary>Creates an instance.</summary>
                    /// <param name="dependency">The dependency.</param>
                    public Example(object dependency) {}
                }
                """,
                """
                class Example
                {
                    private readonly object _dependency;
                    /// <summary>Creates an instance.</summary>
                    /// <param name="dependency">The dependency.</param>
                    public Example(object dependency) { _dependency = dependency; }
                }
                """,
                "_dependency"
            );

            foreach (var typeKind in new[] { "class", "struct", "record" })
            {
                yield return new(
                    "ContainingType_" + typeKind,
                    $$"""
                    class Other { public Other() {} }
                    {{typeKind}} Example { private readonly object _dependency; public Example() {} }
                    """,
                    $$"""
                    class Other { public Other() {} }
                    {{typeKind}} Example { private readonly object _dependency; public Example(object dependency) { _dependency = dependency; } }
                    """,
                    "_dependency"
                );
            }
        }
    }
}
