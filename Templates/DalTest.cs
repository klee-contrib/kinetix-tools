using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// TODO adapter nom du projet
namespace MonProjet.Business.Common.Test
{
    /// <summary>
    /// Defines methods needed in tests of Data Access Layers.
    /// </summary>
    public abstract class DalTest
    {
        private ServiceScope _scope;

        /// <summary>
        /// Publie le singleton des valeurs factices.
        /// </summary>
        public DummyValues Dum => DummyValues.Instance;

        /// <summary>
        /// Test cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            _scope.Dispose();
        }

        /// <summary>
        /// Test initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            _scope = TestUtil.Provider.GetService<TransactionScopeManager>().EnsureTransaction();
        }
    }
}
