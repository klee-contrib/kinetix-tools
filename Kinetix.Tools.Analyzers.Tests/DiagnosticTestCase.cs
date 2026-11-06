using Microsoft.CodeAnalysis;

namespace Kinetix.Tools.Analyzers.Tests;

public sealed record DiagnosticTestCase(string Name, string Source, string FixedSource, string LocationText)
{
    public TestDocument[] AdditionalDocuments { get; init; } = [];
    public string ProjectName { get; init; } = "AnalyzerTest";
    public DiagnosticSeverity Severity { get; init; } = DiagnosticSeverity.Warning;
    public string[] MessageContains { get; init; } = [];
    public int ActionIndex { get; init; }
    public int ActionCount { get; init; } = 1;
    public bool DiagnosticRemains { get; init; }
    public string? TestProjectSupport { get; init; }
    public TestDocument[] TestProjectDocuments { get; init; } = [];
    public GeneratedTestDocument? GeneratedDocument { get; init; }

    public override string ToString() => Name;
}

public sealed record TestDocument(string Name, string Source, string FixedSource)
{
    public string[] Folders { get; init; } = [];
}

public sealed record GeneratedTestDocument(string Name, string Source, string[] Folders);
