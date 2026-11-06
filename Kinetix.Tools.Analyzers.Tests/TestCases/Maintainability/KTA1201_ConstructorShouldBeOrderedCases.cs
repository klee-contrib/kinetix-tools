namespace Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

public static class KTA1201_ConstructorShouldBeOrderedCases
{
    private const string Guard = "if (a < 0) throw new System.ArgumentOutOfRangeException(nameof(a)); ";

    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            yield return Create("AssignmentNames", "_z = z; _a = a;", "_a = a; _z = z;", twoFields: true);
            yield return Create(
                "AssignmentBeforeOtherStatements",
                "System.Console.WriteLine(a); _a = a;",
                "_a = a; System.Console.WriteLine(a);"
            );
            yield return Create("GuardBeforeAssignment", "_a = a; " + Guard, Guard + "_a = a;");
            yield return Create(
                "AssignmentsImmediatelyAfterGuard",
                Guard + "System.Console.WriteLine(a); _a = a;",
                Guard + "_a = a; System.Console.WriteLine(a);"
            );
            yield return Create(
                "MultipleAssignmentsBeforeOtherStatements",
                "System.Console.WriteLine(a); _a = a; _z = z;",
                "_a = a; _z = z; System.Console.WriteLine(a);",
                twoFields: true
            );
            yield return Create(
                "GuardAndAssignmentNames",
                "_z = z; " + Guard + "_a = a;",
                Guard + "_a = a; _z = z;",
                twoFields: true
            );
        }
    }

    private static DiagnosticTestCase Create(string name, string body, string fixedBody, bool twoFields = false)
    {
        var prefix = twoFields
            ? "class Example { private int _a; private int _z; public Example(int a, int z) { "
            : "class Example { private int _a; public Example(int a) { ";
        return new(name, prefix + body + " } }", prefix + fixedBody + " } }", "Example");
    }
}
