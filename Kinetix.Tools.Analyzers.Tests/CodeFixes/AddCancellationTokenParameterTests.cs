using Kinetix.Tools.Analyzers.CodeFixes;
using Kinetix.Tools.Analyzers.Diagnostics.Design;
using Kinetix.Tools.Analyzers.Tests.TestCases.Design;

namespace Kinetix.Tools.Analyzers.Tests.CodeFixes;

[TestClass]
public sealed class AddCancellationTokenParameterTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1104_AsyncMethodShouldHaveCancellationTokenCases.All),
        typeof(KTA1104_AsyncMethodShouldHaveCancellationTokenCases)
    )]
    public Task AppliesExpectedFix(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertFixAsync(
            testCase,
            new KTA1104_AsyncMethodShouldHaveCancellationToken(),
            new AddCancellationTokenParameter()
        );
}
