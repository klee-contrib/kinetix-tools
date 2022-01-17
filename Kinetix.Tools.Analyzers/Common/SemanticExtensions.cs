using System.Linq;
using Microsoft.CodeAnalysis;

namespace Kinetix.Tools.Analyzers.Common
{
    /// <summary>
    /// Extensions pour les symboles du modèle sémantique.
    /// </summary>
    public static class SemanticExtensions
    {
        private const string RegisterContractAttributeName = "Kinetix.Services.Annotations.RegisterContractAttribute";
        private const string RegisterImplAttributeName = "Kinetix.Services.Annotations.RegisterImplAttribute";

        /// <summary>
        /// Renvoie le nom de l'application d'une assemblée.
        /// Chaine.ReferentielImplementation => Chaine.
        /// </summary>
        /// <param name="symbol">Symbole de l'assemblée.</param>
        /// <returns>Nom de l'application.</returns>
        public static string GetApplicationName(this IAssemblySymbol symbol)
        {
            return symbol.Name.Split('.').First();
        }

        /// <summary>
        /// Indique si une assemblée est une implémentation de module métier.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si l'assemblée est une implémentation de module métier.</returns>
        public static bool IsBusinessImplementationAssembly(this IAssemblySymbol symbol)
        {
            return symbol != null
                && !string.IsNullOrEmpty(symbol.Name)
                && symbol.Name.EndsWith("Implementation", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Indique si un symbole est une implémentation de DAL.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est une implémentation de DAL.</returns>
        public static bool IsDalImplementation(this INamedTypeSymbol symbol)
        {
            return symbol != null && symbol.TypeKind == TypeKind.Class
                && (symbol.Name == "AbstractDal"
                    || symbol.Name.StartsWith("Dal", System.StringComparison.Ordinal)
                        && symbol.GetAttributes().Any(a => a.AttributeClass?.ToString() == RegisterImplAttributeName));
        }

        /// <summary>
        /// Indique si le symbole est un contrat de service WCF.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est un contrat de service WCF.</returns>
        public static bool IsServiceContract(this INamedTypeSymbol symbol)
        {
            return symbol != null && symbol.TypeKind == TypeKind.Interface
                && symbol.GetAttributes().Any(a => a.AttributeClass?.ToString() == RegisterContractAttributeName);
        }

        /// <summary>
        /// Indique si un symbole est une implémentation de service WCF.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est une implémentation de service WCF.</returns>
        public static bool IsServiceImplementation(this INamedTypeSymbol symbol)
        {
            if (symbol == null || symbol.TypeKind != TypeKind.Class)
            {
                return false;
            }

            var isRegisterImpl = symbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToString() == RegisterImplAttributeName);

            var hasContract = symbol.AllInterfaces.Any(i => i.GetAttributes().Any(a => a.AttributeClass?.ToString() == RegisterContractAttributeName));

            return isRegisterImpl && hasContract;
        }

        /// <summary>
        /// Indique si deux symboles ont la même mignature.
        /// </summary>
        /// <param name="left">Méthode de gauche.</param>
        /// <param name="right">Méthode de droite.</param>
        /// <returns><code>True</code> si les méthodes ont la même signature.</returns>
        public static bool SignatureEquals(this IMethodSymbol left, IMethodSymbol right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            /* Compare le nom des méthodes. */
            if (left.Name != right.Name)
            {
                return false;
            }

            /* Compare le type de retour. */
            if (left.ReturnType.ToString() != right.ReturnType.ToString())
            {
                return false;
            }

            /* Compare les listes de paramètres */
            var leftParams = left.Parameters;
            var rightParams = right.Parameters;
            if (leftParams.Length != rightParams.Length)
            {
                return false;
            }

            for (var i = 0; i < leftParams.Length; ++i)
            {
                var leftParam = leftParams[i];
                var rightParam = rightParams[i];
                if (leftParam.Type.ToString() != rightParam.Type.ToString())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
