using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MonProjet.AccueilImplementation.Test
{
    [TestClass]
    public static class TestInitializer
    {
        [AssemblyCleanup]
        public static void Cleanup()
        {
            TestUtil.Provider.Dispose();
        }

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            TestUtil.Provider = TestUtil.GetServiceCollection(testContext)
                // TOOD configure project services
                .BuildServiceProvider();
        }
    }
}
