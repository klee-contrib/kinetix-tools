using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Tools.Analyzers.CodeFixes.Test
{
    /// <summary>
    /// Représente une méthode de DAL.
    /// </summary>
    public class DalMethodItem
    {
        /// <summary>
        /// Nom de l'assemblée qui contient la DAL.
        /// </summary>
        public required string DalAssemblyName
        {
            get;
            set;
        }

        /// <summary>
        /// Nom de la classe de la DAL.
        /// </summary>
        public required string DalClassName
        {
            get;
            set;
        }

        /// <summary>
        /// Nom de la méthode de la DAL.
        /// </summary>
        public required string DalMethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Espace de nom de la classe de la DAL.
        /// </summary>
        public required string DalNamespace
        {
            get;
            set;
        }

        /// <summary>
        /// Liste des paramètres de la méthode de la DAL.
        /// </summary>
        public required ICollection<DalMethodParam> Params
        {
            get;
            set;
        }

        /// <summary>
        /// Liste à plat des valeurs des paramètres.
        /// </summary>
        public string FlatParams => string.Join(", ", Params.Select(x => x.Value));

        /// <summary>
        /// Liste des usings spécifiques aux projets.
        /// </summary>
        public required ICollection<string> SpecificUsings
        {
            get;
            set;
        }
    }
}