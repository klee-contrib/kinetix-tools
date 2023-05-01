using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kinetix.Tools.Analyzers.Common
{
    /// <summary>
    /// Extensions pour les objets du modèle syntaxique.
    /// </summary>
    public static class SyntaxExtensions
    {
        private static readonly Regex _dalImplementationFileRegex = new(@"\\DAL\.Implementation\\Dal[^\\]*.cs");

        /// <summary>
        /// Obtient le nom d'une classe.
        /// </summary>
        /// <param name="classNode">Node de la classe.</param>
        /// <returns>Nom non qualifié de la classe.</returns>
        public static string GetClassName(this ClassDeclarationSyntax classNode)
        {
            return classNode.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la localisation du nom de la méthode.
        /// </summary>
        /// <param name="methNode">Node de la méthode.</param>
        /// <returns>Localisation.</returns>
        public static Location GetMethodLocation(this MethodDeclarationSyntax methNode)
        {
            return methNode.ChildTokens()
                .First(x => x.IsKind(SyntaxKind.IdentifierToken))
                .GetLocation();
        }

        /// <summary>
        /// Obtient la localisation du nom du node de déclaration.
        /// </summary>
        /// <param name="node">Node de la classe.</param>
        /// <returns>Localisation.</returns>
        public static Location GetNameDeclarationLocation(this SyntaxNode node)
        {
            return node.ChildTokens()
                .First(x => x.IsKind(SyntaxKind.IdentifierToken))
                .GetLocation();
        }

        /// <summary>
        /// Obtient le nom de l'espace de nom d'une classe.
        /// </summary>
        /// <param name="classNode">Node de la classe.</param>
        /// <returns>Nom complet de l'espace de nom.</returns>
        public static string GetNameSpaceFullName(this ClassDeclarationSyntax classNode)
        {
            var nsName = classNode.FirstAncestorOrSelf<NamespaceDeclarationSyntax>()?.Name
                ?? classNode.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>()?.Name;
            return nsName?.ToString();
        }

        /// <summary>
        /// Indique si un document est un fichier d'implémentation de DAL.
        /// </summary>
        /// <param name="tree">Arbre du document.</param>
        /// <returns><code>True</code> si fichier d'implémentation de DAL.</returns>
        public static bool IsDalImplementationFile(this SyntaxTree tree)
        {
            var filePath = tree.FilePath;
            return !string.IsNullOrEmpty(filePath) && _dalImplementationFileRegex.IsMatch(filePath);
        }

        /// <summary>
        /// Indique si une méthode est publique.
        /// </summary>
        /// <param name="methNode">Node de la méthode.</param>
        /// <returns><code>True</code> si la méthode est publique.</returns>
        public static bool IsPublic(this MethodDeclarationSyntax methNode)
        {
            return methNode.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword));
        }

        /// <summary>
        /// Obtient le nom d'un node de field.
        /// </summary>
        /// <param name="node">Node de field.</param>
        /// <returns>Nom du field.</returns>
        public static string GetFieldName(this FieldDeclarationSyntax node)
        {
            /* Suppose qu'il n'y a qu'une seule variable. */
            return node.Declaration.Variables.First().Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la location d'un nom de field.
        /// </summary>
        /// <param name="node">Node de field.</param>
        /// <returns>Location du field.</returns>
        public static Location GetFieldNameLocation(this FieldDeclarationSyntax node)
        {
            /* Suppose qu'il n'y a qu'une seule variable. */
            return node.Declaration.Variables.First().Identifier.GetLocation();
        }

        /// <summary>
        /// Obtient le nom d'un node de paramètre.
        /// </summary>
        /// <param name="node">Node de paramètre.</param>
        /// <returns>Nom du paramètre.</returns>
        public static string GetParameterName(this ParameterSyntax node)
        {
            return node.Identifier.ValueText;
        }

        /// <summary>
        /// Obtient la location d'un nom de paramètre.
        /// </summary>
        /// <param name="node">Node de paramètre.</param>
        /// <returns>Location du paramètre.</returns>
        public static Location GetParameterNameLocation(this ParameterSyntax node)
        {
            return node.Identifier.GetLocation();
        }
    }
}
