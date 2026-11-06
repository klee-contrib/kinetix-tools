using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kinetix.Tools.Analyzers.Tests;

internal static class DiagnosticCaseAssertions
{
    public static async Task AssertDiagnosticAsync(DiagnosticTestCase testCase, DiagnosticAnalyzer analyzer)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(testCase, analyzer);
        AnalyzerTestHelper.AssertDiagnostic(
            diagnostics,
            analyzer.SupportedDiagnostics.Single().Id,
            testCase.LocationText,
            testCase.Severity
        );
        foreach (var expected in testCase.MessageContains)
        {
            StringAssert.Contains(diagnostics[0].GetMessage(), expected);
        }
    }

    public static async Task AssertFixAsync(
        DiagnosticTestCase testCase,
        DiagnosticAnalyzer analyzer,
        CodeFixProvider provider
    )
    {
        using var context = new CodeFixTestContext(testCase.Source, analyzer, provider, testCase.ProjectName);
        var documents = new Dictionary<DocumentId, TestDocument>();
        foreach (var document in testCase.AdditionalDocuments)
        {
            documents.Add(context.AddDocument(document.Name, document.Source, folders: document.Folders), document);
        }

        ProjectId? testProjectId = null;
        if (testCase.TestProjectSupport != null)
        {
            testProjectId = context.AddTestProject(testCase.TestProjectSupport);
            foreach (var document in testCase.TestProjectDocuments)
            {
                documents.Add(
                    context.AddDocument(document.Name, document.Source, testProjectId, document.Folders),
                    document
                );
            }
        }
        else
        {
            Assert.IsEmpty(testCase.TestProjectDocuments);
            Assert.IsNull(testCase.GeneratedDocument);
        }

        var originalDocuments = context.Solution.Projects.SelectMany(p => p.DocumentIds).ToHashSet();
        if (testCase.ActionCount == 0)
        {
            Assert.IsEmpty(await context.GetActionsAsync());
        }
        else
        {
            await context.ApplyAsync(testCase.ActionIndex, testCase.ActionCount);
        }

        await context.AssertSourceAsync(testCase.FixedSource);
        foreach (var (id, document) in documents)
        {
            await context.AssertSourceAsync(document.FixedSource, id);
        }

        var generated = context
            .Solution.Projects.SelectMany(p => p.Documents)
            .Where(d => !originalDocuments.Contains(d.Id))
            .ToArray();
        if (testCase.GeneratedDocument is { } expectedDocument)
        {
            Assert.HasCount(1, generated);
            Assert.AreEqual(testProjectId, generated[0].Project.Id);
            Assert.AreEqual(expectedDocument.Name, generated[0].Name);
            CollectionAssert.AreEqual(expectedDocument.Folders, generated[0].Folders.ToArray());
            await context.AssertSourceAsync(expectedDocument.Source, generated[0].Id);
            Assert.IsEmpty(await context.GetActionsAsync());
        }
        else
        {
            Assert.IsEmpty(generated);
        }

        if (testCase.DiagnosticRemains)
        {
            AnalyzerTestHelper.AssertDiagnostic(
                await context.GetDiagnosticsAsync(),
                analyzer.SupportedDiagnostics.Single().Id,
                testCase.LocationText,
                testCase.Severity
            );
        }
        else
        {
            await context.AssertNoDiagnosticsAsync();
        }
    }
}
