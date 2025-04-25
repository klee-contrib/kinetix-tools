using Kinetix.Modeling;
using Kinetix.Modeling.Annotations;


namespace Kinetix.Tools.TestUtils
{
    /// <summary>
    /// Utilitaire publiant des valeurs factices pour les tests.
    /// </summary>
    public class DummyValues
    {
        /// <summary>
        /// Singleton.
        /// </summary>
        public static readonly DummyValues Instance = new();

        /// <summary>
        /// Valeur factice pour un booléen.
        /// </summary>
        public bool Booleen = true;

        /// <summary>
        /// Valeur factice pour une chaîne.
        /// </summary>
        public string Code = "T";

        /// <summary>
        /// Valeur factice pour une chaîne de longueur 1.
        /// </summary>
        public string Code1 = "T";

        /// <summary>
        /// Valeur factice pour une date.
        /// </summary>
        public DateTime Date = DateTime.UtcNow.Date;

        /// <summary>
        /// Valeur factice pour un GUID.
        /// </summary>
        public Guid Guid = Guid.Parse("fecad091-a122-4d3b-b583-e254f073f4f3");

        /// <summary>
        /// Valeur factice pour un entier.
        /// </summary>
        public int Id = 1;

        /// <summary>
        /// Valeur factice de tableau d'id.
        /// </summary>
        public int[] IdArray = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        /// <summary>
        /// Valeur factice de tableau d'id.
        /// </summary>
        public Guid[] GuidArray = { Guid.Parse("fecad091-a122-4d3b-b583-e254f073f4f3"), Guid.Parse("680d3265-763e-4bf5-a976-0000c35f0dce") };

        /// <summary>
        /// Valeur factice de list d'id.
        /// </summary>
        public ICollection<int> IdList = new List<int> { 1, 2 };

        /// <summary>
        /// Valeur factice pour une chaîne.
        /// </summary>
        public string Libelle = "TEST";

        /// <summary>
        /// Valeur factice pour un montant.
        /// </summary>
        public decimal Montant = 10.00m;

        /// <summary>
        /// Valeur factice pour un pourcentage.
        /// </summary>
        public decimal Pourcentage = 5.00m;

        /// <summary>
        /// Valuer factice de tableau de string.
        /// </summary>
        public string[] StringArray = { "T" };

        /// <summary>
        /// Valuer factice de list de string.
        /// </summary>
        public ICollection<string> StringList = new List<string> { "T" };

        /// <summary>
        /// Renvoie un bean du type demandé avec des valeurs factices.
        /// </summary>
        /// <typeparam name="T">Type du bean.</typeparam>
        /// <param name="withId">Avec id?</param>
        /// <returns>Bean.</returns>
        public T Dum<T>(bool withId = true)
            where T : new()
        {
            var dto = new T();
            /* génère des valeurs pour les champs primitifs */
            foreach (var prop in BeanDescriptor.GetDefinition(typeof(T)).Properties)
            {
                if (!prop.IsReadOnly && prop.PrimitiveType != null && !(prop.IsPrimaryKey && !withId))
                {
                    if (prop.Domain.ValidationAttributes.OfType<EmailAttribute>().Any())
                    {
                        prop.SetValue(dto, "yolo@email.com");
                    }
                    else
                    {
                        prop.SetValue(dto, Dum(prop.PrimitiveType));
                    }
                }
            }

            return dto;
        }

        /// <summary>
        /// Renvoie une valeur factice pour un type primitif donné.
        /// </summary>
        /// <param name="t">Type.</param>
        /// <returns>Valeur.</returns>
        public object Dum(Type t)
        {
            return t == typeof(string)
                ? Code
                : t == typeof(int)
                ? Id
                : t == typeof(short)
                ? Convert.ToInt16(Id)
                : t == typeof(bool)
                ? Booleen
                : t == typeof(DateTime)
                ? Date
                : t == typeof(decimal)
                ? Montant
                : t == typeof(Guid)
                ? Guid
                : (object)null;
        }

        /// <summary>
        /// Renvoie une liste avec un bean du type demandé avec des valeurs factices.
        /// </summary>
        /// <typeparam name="T">Type du bean.</typeparam>
        /// <param name="withId">Avec id?</param>
        /// <returns>Bean.</returns>
        public ICollection<T> DumColl<T>(bool withId = true)
            where T : new()
        {
            return new List<T> { Dum<T>(withId) };
        }

        /// <summary>
        /// Renvoie une liste avec un bean du type demandé avec des valeurs factices.
        /// Sur le bean, l'ID est mis à null pour permettre une insertion.
        /// </summary>
        /// <typeparam name="T">Type du bean.</typeparam>
        /// <returns>Bean.</returns>
        public ICollection<T> DumNewColl<T>()
            where T : class, new()
        {
            return new List<T> { Dum<T>(false) };
        }
    }
}