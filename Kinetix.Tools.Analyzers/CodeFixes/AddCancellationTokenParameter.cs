using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kinetix.Tools.Analyzers.Common;
using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Kinetix.Tools.Analyzers.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixes.AddCancellationTokenParameter))]
    [Shared]
    public class AddCancellationTokenParameter : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => [KTA1104_AsyncMethodShouldHaveCancellationToken.DiagnosticId];

        /// <summary>
        /// Permet d'effectuer des corrections de masse via le correcteur en lot par défaut.
        /// </summary>
        /// <returns>Le correcteur de masse.</returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Enregistre les corrections de codes.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        /// <returns>Peut être attendu.</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root?.FindNode(diagnosticSpan) is MethodDeclarationSyntax method)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Ajouter un CancellationToken en paramètre",
                        ct => AddParameter(context.Document, method, ct),
                        "Ajouter un CancellationToken en paramètre"),
                    diagnostic);
            }
        }

        private static async Task<Solution> AddParameter(Document document, MethodDeclarationSyntax method, CancellationToken ct)
        {
            var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
            if (root == null)
            {
                return document.Project.Solution;
            }

            var semanticModel = await document.GetSemanticModelAsync(ct);

            var newMethod = AddCtParam(method);

            var newRoot = Formatter.Format(
                root.ReplaceNode(method, newMethod),
                document.Project.Solution.Workspace,
                cancellationToken: ct);

            var solution = document.Project.Solution.WithDocumentSyntaxRoot(document.Id, newRoot);

            var methodModel = semanticModel.GetDeclaredSymbol(method);
            if (methodModel != null)
            {
                var implementedMethod = methodModel.GetImplementedMethod();
                if (implementedMethod != null)
                {
                    var declarationSR = implementedMethod.DeclaringSyntaxReferences.FirstOrDefault();
                    if (declarationSR != null)
                    {
                        var declaration = await declarationSR.GetSyntaxAsync(ct);
                        if (declaration is MethodDeclarationSyntax interfaceMethod)
                        {
                            var interfaceRoot = await interfaceMethod.SyntaxTree.GetRootAsync();

                            var interfaceDocument = solution.GetDocument(interfaceMethod.SyntaxTree);
                            if (interfaceDocument != null)
                            {
                                var newInterfaceMethod = AddCtParam(interfaceMethod);

                                var newInterfaceRoot = Formatter.Format(
                                    interfaceRoot.ReplaceNode(interfaceMethod, newInterfaceMethod),
                                    interfaceDocument.Project.Solution.Workspace,
                                    cancellationToken: ct);

                                solution = solution.WithDocumentSyntaxRoot(interfaceDocument.Id, newInterfaceRoot);
                            }
                        }
                    }
                }
            }

            return solution;
        }

        private static MethodDeclarationSyntax AddCtParam(MethodDeclarationSyntax method)
        {
            return method
                .WithParameterList(
                    method.ParameterList.AddParameters(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.List<AttributeListSyntax>(),
                            SyntaxFactory.TokenList(),
                            SyntaxFactory.ParseTypeName("CancellationToken"),
                            SyntaxFactory.Identifier("ct"),
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression)))))
                .WithLeadingTrivia(
                    method.GetLeadingTrivia()
                        .Select(i =>
                        {
                            if (i.GetStructure() is not DocumentationCommentTriviaSyntax doc || doc.ToString().Contains("inheritdoc", System.StringComparison.InvariantCultureIgnoreCase))
                            {
                                return i;
                            }

                            var previousNode = doc.Content.LastOrDefault(c => c.ToString().Contains("<param"))
                                ?? doc.Content.LastOrDefault(c => c.ToString().Contains("<summary>"));

                            if (previousNode == null)
                            {
                                return i;
                            }

                            return SyntaxFactory.Trivia(
                                doc.InsertNodesAfter(
                                    previousNode,
                                    [
                                        SyntaxFactory.XmlText(SyntaxFactory.TokenList(SyntaxFactory.XmlTextNewLine("\r\n", false))),
                                        SyntaxFactory.XmlText(
                                            SyntaxFactory.TokenList(
                                                SyntaxFactory.XmlTextLiteral(
                                                    SyntaxFactory.TriviaList(SyntaxFactory.DocumentationCommentExterior("/// ")),
                                                    string.Empty,
                                                    string.Empty,
                                                    SyntaxFactory.TriviaList()))),
                                        SyntaxFactory.XmlElement(
                                            SyntaxFactory.XmlElementStartTag(
                                                SyntaxFactory.XmlName("param"),
                                                SyntaxFactory.List(new List<XmlAttributeSyntax> { SyntaxFactory.XmlNameAttribute("ct") })),
                                            SyntaxFactory.List(
                                                new List<XmlNodeSyntax>
                                                {
                                                    SyntaxFactory.XmlText(SyntaxFactory.TokenList(SyntaxFactory.XmlTextLiteral("CancellationToken.")))
                                                }),
                                            SyntaxFactory.XmlElementEndTag(SyntaxFactory.XmlName("param")))
                                    ]));
                        }));
        }
    }
}
