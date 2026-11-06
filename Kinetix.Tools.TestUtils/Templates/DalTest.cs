using Kinetix.Services;
using Kinetix.Tools.TestUtils.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Tools.TestUtils.Templates;

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
        _scope = SharedState.Provider.GetService<TransactionScopeManager>().EnsureTransaction();
    }
}
