using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Kinetix.Tools.Analyzers.Common
{
    /// <summary>
    /// Méthodes d'extensions pour les code fix.
    /// </summary>
    public static class CodeFixExtensions
    {
        /// <summary>
        /// Ajoute un using s'il manque.
        /// </summary>
        /// <param name="rootNode">Node racine.</param>
        /// <param name="nameSpace">Espace de nom à rajouter.</param>
        /// <returns>Node avec le using.</returns>
        public static SyntaxNode AddUsing(this SyntaxNode rootNode, string nameSpace)
        {
            var unitSyntax = (CompilationUnitSyntax)rootNode;
            if (unitSyntax == null)
            {
                return rootNode;
            }

            /* Vérifie que le using n'est pas déjà présent. */
            if (unitSyntax.Usings.Any(x => x.Name.ToString() == nameSpace))
            {
                return rootNode;
            }

            /* Ajoute le using. */
            var name = SyntaxFactory.ParseName(nameSpace);
            unitSyntax = unitSyntax.AddUsings(SyntaxFactory.UsingDirective(name).NormalizeWhitespace());

            //// TODO : ordre alphabétique avec System en premier.

            return unitSyntax;
        }
    }
}
