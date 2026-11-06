using Kinetix.Tools.Analyzers.Diagnostics.Maintainability;
using Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

namespace Kinetix.Tools.Analyzers.Tests.Diagnostics.Maintainability;

[TestClass]
public sealed class KTA1200_ClassMembersShouldBeOrderedTests
{
    [TestMethod]
    [DynamicData(
        nameof(KTA1200_ClassMembersShouldBeOrderedCases.All),
        typeof(KTA1200_ClassMembersShouldBeOrderedCases)
    )]
    public Task ReportsDiagnostic(DiagnosticTestCase testCase) =>
        DiagnosticCaseAssertions.AssertDiagnosticAsync(testCase, new KTA1200_ClassMembersShouldBeOrdered());

    [TestMethod]
    [DataRow("class Example { }")]
    [DataRow("class Example { private int _a; private int _z; private void A() {} private void Z() {} }")]
    [DataRow(
        "class Example { private const int Size = 1; private static readonly int Shared = 1; private int _value; public Example() {} public int Value => _value; public void Run() {} }"
    )]
    [DataRow(
        "using System; class GeneratedCodeAttribute : Attribute {} [GeneratedCode] class Example { void Run() {} private int _value; }"
    )]
    [DataRow("struct Example {}")]
    [DataRow("interface Example { void A(); void Z(); }")]
    [DataRow("enum Example { Z, A }")]
    [DataRow("delegate void Example(int z, int a);")]
    [DataRow("class Example { private int _z, _a; }")]
    [DataRow("partial class Example { public void Run() {} } partial class Example { private int _value; }")]
    [DataRow("class Example { void Run() { var value = new { Z = 1, A = 2 }; } }")]
    [DataRow("[System.CodeDom.Compiler.GeneratedCode(\"Tests\", \"1.0\")] class Example { void Z() {} void A() {} }")]
    [DataRow(
        "using System.CodeDom.Compiler; [GeneratedCode(\"Tests\", \"1.0\")] record Example { void Z() {} void A() {} }"
    )]
    [DataRow("class Example { public void Run(int value) {} public void Run() {} }")]
    [DataRow("class Example { public int Z => 1; public int A => 2; }")]
    [DataRow("class Example { public Example(string value) {} public Example(int value) {} }")]
    [DataRow(
        "class Example { public static Example operator -(Example x, Example y) => x; public static Example operator +(Example x, Example y) => x; }"
    )]
    [DataRow(
        "class Example { public static implicit operator long(Example value) => 0; public static implicit operator int(Example value) => 0; }"
    )]
    [DataRow("static class Example { extension(string value) { public int Size => value.Length; } }")]
    [DataRow(
        "static class Example { extension(string value) { public int Size => value.Length; } public static int Value => 1; public static void A() {} private static void Z() {} }"
    )]
    [DataRow(
        "static class Example { extension(string value) { public int Z() => value.Length; } extension(int value) { public int A() => value; } }"
    )]
    [DataRow(
        "static class Example { extension(string value) { public int Z => 1; public int A => 2; public void Run(int x) {} public void Run() {} } }"
    )]
    [DataRow(
        "[System.CodeDom.Compiler.GeneratedCode(\"Tests\", \"1.0\")] static class Example { extension(string value) { public void Z() {} public void A() {} } }"
    )]
    public async Task DoesNotReportWhenClassMembersAreOrderedOrGenerated(string source)
    {
        var diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, new KTA1200_ClassMembersShouldBeOrdered());

        Assert.IsEmpty(diagnostics);
    }
}
