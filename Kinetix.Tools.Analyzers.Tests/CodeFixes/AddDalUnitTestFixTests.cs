using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Coverage;
using Kinetix.Tools.Analyzers.Tests.TestCases.Coverage;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class AddDalUnitTestFixTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1300_DalMethodWithSqlServerCommandAnalyserCases.All),
        typeof(KTA1300_DalMethodWithSqlServerCommandAnalyserCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1300_DalMethodWithSqlServerCommandAnalyser(),
            new AddDalUnitTestFix()
        );
}
