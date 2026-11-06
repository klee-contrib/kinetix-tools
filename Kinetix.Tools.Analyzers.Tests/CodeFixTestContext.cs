using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Kinetix.Tools.Analyzers.Tests;

internal sealed class CodeFixTestContext : IDisposable
{
    private readonly AdhocWorkspace _workspace = new();
    private readonly DiagnosticAnalyzer _analyzer;
    private readonly CodeFixProvider _provider;

    public CodeFixTestContext(
        string source,
        DiagnosticAnalyzer analyzer,
        CodeFixProvider provider,
        string projectName = "AnalyzerTest"
    )
    {
        _analyzer = analyzer;
        _provider = provider;
        var project = _workspace
            .AddProject(projectName, LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest))
            .AddMetadataReferences(AnalyzerTestHelper.References);
        var document = project.AddDocument("Example.cs", SourceText.From(source), filePath: @"C:\Tests\Example.cs");
        DocumentId = document.Id;
        Assert.IsTrue(_workspace.TryApplyChanges(document.Project.Solution));
    }

    public DocumentId DocumentId { get; }

    public Solution Solution => _workspace.CurrentSolution;

    public DocumentId AddDocument(string name, string source, ProjectId? projectId = null, string[]? folders = null)
    {
        var project = Solution.GetProject(projectId ?? DocumentId.ProjectId)!;
        var document = project.AddDocument(name, SourceText.From(source), folders, @"C:\Tests\" + name);
        Assert.IsTrue(_workspace.TryApplyChanges(document.Project.Solution));
        return document.Id;
    }

    public ProjectId AddTestProject(string supportSource)
    {
        var sourceProject = Solution.GetProject(DocumentId.ProjectId)!;
        var project = Solution
            .AddProject(
                ProjectId.CreateNewId(),
                sourceProject.Name + ".Test",
                sourceProject.AssemblyName + ".Test",
                LanguageNames.CSharp
            )
            .Projects.Single(p => p.Name == sourceProject.Name + ".Test")
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddMetadataReferences(AnalyzerTestHelper.References)
            .AddProjectReference(new ProjectReference(sourceProject.Id));
        Assert.IsTrue(_workspace.TryApplyChanges(project.Solution));
        AddDocument("TestSupport.cs", supportSource, project.Id);
        return project.Id;
    }

    public async Task<ImmutableArray<CodeAction>> GetActionsAsync()
    {
        await AssertCompilesAsync();
        var document = Solution.GetDocument(DocumentId)!;
        var diagnostics = await GetDiagnosticsAsync();
        var tree = await document.GetSyntaxTreeAsync();
        var diagnostic = diagnostics.Single(d => d.Location.SourceTree == tree);
        CollectionAssert.Contains(_provider.FixableDiagnosticIds.ToArray(), diagnostic.Id);
        var actions = ImmutableArray.CreateBuilder<CodeAction>();
        await _provider.RegisterCodeFixesAsync(
            new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None)
        );
        return actions.ToImmutable();
    }

    public async Task ApplyAsync(int actionIndex = 0, int expectedActionCount = 1)
    {
        var actions = await GetActionsAsync();
        Assert.HasCount(expectedActionCount, actions);
        var action = actions[actionIndex];
        Assert.IsFalse(string.IsNullOrWhiteSpace(action.Title));
        Assert.IsFalse(string.IsNullOrWhiteSpace(action.EquivalenceKey));
        var operations = await action.GetOperationsAsync(CancellationToken.None);
        var changes = operations.OfType<ApplyChangesOperation>().ToArray();
        Assert.HasCount(1, changes);
        Assert.IsTrue(_workspace.TryApplyChanges(changes[0].ChangedSolution));

        // Reparse persisted text so malformed tokens or documentation cannot hide in an in-memory syntax tree.
        var solution = Solution;
        foreach (var document in solution.Projects.SelectMany(p => p.Documents))
        {
            var text = await document.GetTextAsync();
            solution = solution.WithDocumentText(document.Id, SourceText.From(text.ToString()));
        }
        Assert.IsTrue(_workspace.TryApplyChanges(solution));
        await AssertCompilesAsync();
    }

    public async Task AssertSourceAsync(string expectedSource, DocumentId? documentId = null)
    {
        var text = await Solution.GetDocument(documentId ?? DocumentId)!.GetTextAsync();
        Assert.AreEqual(
            CSharpSyntaxTree.ParseText(expectedSource).GetRoot().NormalizeWhitespace().ToFullString(),
            CSharpSyntaxTree.ParseText(text).GetRoot().NormalizeWhitespace().ToFullString()
        );
    }

    public async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync()
    {
        var compilation = await Solution.GetProject(DocumentId.ProjectId)!.GetCompilationAsync();
        Assert.IsNotNull(compilation);
        return await compilation.WithAnalyzers([_analyzer]).GetAnalyzerDiagnosticsAsync();
    }

    public async Task AssertNoDiagnosticsAsync()
    {
        var diagnostics = await GetDiagnosticsAsync();
        Assert.IsEmpty(diagnostics, string.Join(Environment.NewLine, diagnostics));
    }

    private async Task AssertCompilesAsync()
    {
        foreach (var project in Solution.Projects)
        {
            var compilation = await project.GetCompilationAsync();
            Assert.IsNotNull(compilation);
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(d => d.ToString())));
        }
    }

    public void Dispose() => _workspace.Dispose();
}
