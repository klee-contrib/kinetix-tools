using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kinetix.Tools.Analyzers.Tests;

internal static class AnalyzerTestHelper
{
    internal static readonly ImmutableArray<MetadataReference> References =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)),
    ];

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        DiagnosticAnalyzer analyzer,
        string assemblyName = "AnalyzerTest",
        params MetadataReference[] additionalReferences
    )
    {
        var compilation = CreateCompilation(source, assemblyName, additionalReferences);
        return await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
    }

    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        DiagnosticTestCase testCase,
        DiagnosticAnalyzer analyzer
    )
    {
        IEnumerable<SyntaxTree> trees = [CSharpSyntaxTree.ParseText(testCase.Source, path: "Example.cs")];
        trees = trees.Concat(
            testCase.AdditionalDocuments.Select(document =>
                CSharpSyntaxTree.ParseText(document.Source, path: document.Name)
            )
        );
        var compilation = CreateCompilation(trees, testCase.ProjectName);
        return await compilation.WithAnalyzers([analyzer]).GetAnalyzerDiagnosticsAsync();
    }

    public static MetadataReference CreateReference(string source, string assemblyName) =>
        CreateCompilation(source, assemblyName).ToMetadataReference();

    public static void AssertDiagnostic(
        ImmutableArray<Diagnostic> diagnostics,
        string diagnosticId,
        string locationText,
        DiagnosticSeverity severity = DiagnosticSeverity.Warning
    )
    {
        Assert.HasCount(1, diagnostics);
        var diagnostic = diagnostics[0];
        Assert.AreEqual(diagnosticId, diagnostic.Id);
        Assert.AreEqual(severity, diagnostic.Severity);
        Assert.AreEqual(
            locationText,
            diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan)
        );
    }

    private static CSharpCompilation CreateCompilation(
        string source,
        string assemblyName,
        params MetadataReference[] additionalReferences
    ) => CreateCompilation([CSharpSyntaxTree.ParseText(source)], assemblyName, additionalReferences);

    private static CSharpCompilation CreateCompilation(
        IEnumerable<SyntaxTree> trees,
        string assemblyName,
        params MetadataReference[] additionalReferences
    )
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            trees,
            References.AddRange(additionalReferences),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        AssertCompiles(compilation);
        return compilation;
    }

    private static void AssertCompiles(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
    }
}
