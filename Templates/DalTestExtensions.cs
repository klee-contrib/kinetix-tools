using Kinetix.DataAccess.Sql;
using Kinetix.DataAccess.Sql.Broker;
using Kinetix.Modeling.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

// TODO adapter l'espace de nom.
namespace MonProjet.Business.Common.Test
{
    /// <summary>
    /// Méthodes d'extensions des tests de DAL.
    /// </summary>
    public static class DalTestExtensions
    {
        /// <summary>
        /// Test un appel de DAL en ne vérifiant que la syntaxe et le modèle.
        /// Les erreurs dues aux données ne mettent pas le test en échec :
        ///   - ZeroRowException
        ///   - Foreign Key.
        /// </summary>
        /// <param name="_">Classe de test.</param>
        /// <param name="action">Action pour appeler la DAL.</param>
        public static void CheckDalSyntax<TDal>(this DalTest _, Action<TDal> action)
        {
            try
            {
                action(TestUtil.Provider.GetService<TDal>());
            }
            catch (BrokerException be)
            {
                /* Cas liés aux données : le test passe. */
                switch (be.Message)
                {
                    case "Zero row deleted":
                    case "Zero record affected":
                        return;
                }

                /* Autres cas : on relance l'exception */
                throw;
            }
            catch (CollectionBuilderException cbe)
            {
                /* Cas liés aux données : le test passe. */
                switch (cbe.Message)
                {
                    case "Zero row selected !":
                        return;
                }

                /* Autres cas : on relance l'exception */
                throw;
            }
            catch (PostgresException se)
            {
                if (HandleSqlException(se))
                {
                    return;
                }

                throw;
            }
            catch (SqlDataException se)
            {
                if (se.InnerException is PostgresException sqlException)
                {
                    if (HandleSqlException(sqlException))
                    {
                        return;
                    }
                }

                throw;
            }
            catch (BusinessException ce)
            {
                if (ce.InnerException is PostgresException sqlException)
                {
                    if (HandleSqlException(sqlException))
                    {
                        return;
                    }
                }

                throw;
            }
            catch (NotSupportedException nse)
            {
                /* Cas d'un ExecuteScalar qui renvoie null : le test passe. */
                if (nse.Message == "Null result is not supported.")
                {
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// Gère une exception SQL.
        /// </summary>
        /// <param name="sqlException">Exception SQL.</param>
        /// <returns><code>True</code> si le test passe.</returns>
        private static bool HandleSqlException(PostgresException sqlException)
        {
            /* Cas d'une violation de clé étrangère : le test passe. */
            return sqlException.ConstraintName != null;
        }
    }
}
