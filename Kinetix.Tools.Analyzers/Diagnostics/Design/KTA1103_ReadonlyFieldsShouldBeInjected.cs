using System.Collections.Immutable;
using System.Linq;
using Kinetix.Tools.Analyzers.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Kinetix.Tools.Analyzers.Diagnostics.Design
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class KTA1103_ReadonlyFieldsShouldBeInjected : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "KTA1103";
        private const string Category = "Design";
        private static readonly string Description = "Les champs en lecture seule doivent être initialisés ou injectés dans le constructeur.";
        private static readonly string MessageFormat = "Le champ en lecture seule n'est pas initialisé";
        private static readonly string Title = "Les champs en lecture seule doivent être initialisés ou injectés dans le constructeur";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <summary>
        /// Méthode d'initialisation de l'analyseur.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyserChamp, SymbolKind.Field);
        }

        /// <summary>
        /// Trouve des diagnostics sur un champ.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        private static void AnalyserChamp(SymbolAnalysisContext context)
        {
            // On récupère les informations nécessaires du contexte du symbole.
            var location = context.Symbol.Locations.First();
            var racine = location.SourceTree.GetRoot();
            var modèleSémantique = context.Compilation.GetSemanticModel(location.SourceTree);

            // On vérifie que le champ est bien en lecture seule et n'est pas initialisé à la déclaration.
            if (racine.FindNode(location.SourceSpan) is not VariableDeclaratorSyntax déclarationChamp || context.Symbol is IFieldSymbol { IsReadOnly: false } || déclarationChamp.Initializer != null)
            {
                return;
            }

            // On parcourt tous les constructeurs de la classe et récupère les assignations du champ dans chacun.
            var usages = racine.FindNode(déclarationChamp.Ancestors().OfType<ClassDeclarationSyntax>().First().Span)
                .ChildNodes().OfType<ConstructorDeclarationSyntax>()
                .SelectMany(constructeur =>
                    constructeur.DescendantNodes()
                        .Where(x =>
                        {
                            var assignation = x as AssignmentExpressionSyntax;
                            return assignation?.Left != null && SymbolEqualityComparer.Default.Equals(modèleSémantique.GetSymbolInfo(assignation.Left).Symbol, context.Symbol);
                        }).Concat(
                    constructeur.DescendantNodes()
                        .Where(x =>
                        {
                            var argument = x as ArgumentSyntax;
                            return (argument?.RefKindKeyword.IsKind(SyntaxKind.OutKeyword) ?? false) && SymbolEqualityComparer.Default.Equals(modèleSémantique.GetSymbolInfo(argument.Expression).Symbol, context.Symbol);
                        })));

            // Si le champ n'est jamais initialisé, on lève l'erreur.
            if (!usages.Any())
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0]));
            }
        }
    }
}