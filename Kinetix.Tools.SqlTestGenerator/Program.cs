﻿using Kinetix.Tools.Analyzers.CodeFixes.Test;
using Kinetix.Tools.Analyzers.Common;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

// Lecture des paramètres d'entrée.
var solutionFile = args[0];
if (!File.Exists(solutionFile))
{
    throw new FileNotFoundException("Le fichier de solution est introuvable");
}

var instance = MSBuildLocator.QueryVisualStudioInstances().First();
Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
MSBuildLocator.RegisterInstance(instance);
var msWorkspace = MSBuildWorkspace.Create();

msWorkspace.WorkspaceFailed += MsWorkspace_WorkspaceFailed;
var solution = await msWorkspace.OpenSolutionAsync(solutionFile);

var docs = solution.Projects
    .Where(projet => projet.AssemblyName.EndsWith("Implementation"))
    .SelectMany(p => p.Documents)
    .ToList();

await Task.WhenAll(docs.Select(async doc =>
{
    var syntaxTree = await doc.GetSyntaxTreeAsync();
    if (!syntaxTree.IsDalImplementationFile())
    {
        return;
    }

    var classDecl = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
    if (classDecl == null)
    {
        return;
    }

    var semanticModel = await doc.GetSemanticModelAsync();
    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

    if (!classSymbol.IsDalImplementation())
    {
        return;
    }

    var testProject = solution.Projects.SingleOrDefault(p => p.Name == $"{doc.Project.Name}.Test");
    if (testProject == null)
    {
        return;
    }

    var testProjectFileInfo = new FileInfo(testProject.FilePath);

    foreach (var methDecl in classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>())
    {
        if (!methDecl.IsPublic())
        {
            continue;
        }

        var methSymbol = semanticModel.GetDeclaredSymbol(methDecl) as IMethodSymbol;

        if (!TestGenerator.ShouldGenerateTest(methSymbol, classSymbol, doc))
        {
            continue;
        }

        var (fileName, folder, content) = TestGenerator.GenerateTest(methSymbol, classDecl, DalTestStrategy.Semantic);
        var fullPath = $"{testProjectFileInfo.Directory}\\{folder}\\{fileName}.cs";

        Directory.CreateDirectory($"{testProjectFileInfo.Directory}\\{folder}");
        File.WriteAllText(fullPath, content);
        Console.WriteLine($"{folder}/{fileName} généré avec succès");
    }
}));


static void MsWorkspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e)
{
    Console.WriteLine(e.Diagnostic.Message);
}
