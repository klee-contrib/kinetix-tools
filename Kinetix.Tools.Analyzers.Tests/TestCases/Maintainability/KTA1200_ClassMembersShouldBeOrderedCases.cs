namespace Kinetix.Tools.Analyzers.Tests.TestCases.Maintainability;

public static class KTA1200_ClassMembersShouldBeOrderedCases
{
    private static readonly (string Name, string Source)[] MemberKinds =
    [
        ("Constant", "private const int Size = 1;"),
        ("Field", "private int _value;"),
        ("Constructor", "public Example() {}"),
        ("Destructor", "~Example() {}"),
        ("Delegate", "public delegate void Callback();"),
        ("Event", "public event System.Action Changed;"),
        ("Enum", "public enum Mode { Default }"),
        ("Interface", "public interface IWorker {}"),
        ("Property", "public int Value => 1;"),
        ("Indexer", "public int this[int index] => index;"),
        ("Conversion", "public static implicit operator int(Example value) => 1;"),
        ("Operator", "public static Example operator +(Example left, Example right) => left;"),
        ("Method", "public void Run() {}"),
        ("Struct", "public struct State {}"),
        ("Class", "public class Nested {}"),
        ("Record", "public record Snapshot;"),
    ];

    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            yield return Create(
                "MethodBeforeField",
                "void Run() {} private int _value;",
                "private int _value; void Run() {}"
            );
            yield return Create("FieldNames", "private int _z; private int _a;", "private int _a; private int _z;");
            yield return Create(
                "MethodNames",
                "private void Z() {} private void A() {}",
                "private void A() {} private void Z() {}"
            );
            yield return Create(
                "Accessibility",
                "private void A() {} public void Z() {}",
                "public void Z() {} private void A() {}"
            );
            yield return Create(
                "ConstructorBeforeMethod",
                "public void Run() {} public Example() {}",
                "public Example() {} public void Run() {}"
            );
            yield return Create(
                "ConstructorArity",
                "public Example(int value) {} public Example() {}",
                "public Example() {} public Example(int value) {}"
            );

            foreach (
                var testCase in MemberCategoryCases()
                    .Concat(AccessibilityCases())
                    .Concat(ModifierCases())
                    .Concat(NameCases())
                    .Concat(TypeCases())
                    .Concat(StabilityCases())
                    .Concat(CustomEventCases())
                    .Concat(ExtensionBlockCases())
                    .Concat(ExtensionMemberCases())
            )
            {
                yield return testCase;
            }
        }
    }

    private static IEnumerable<DiagnosticTestCase> MemberCategoryCases()
    {
        for (var index = 0; index < MemberKinds.Length - 1; index++)
        {
            var (name1, source1) = MemberKinds[index];
            var (name2, source2) = MemberKinds[index + 1];
            yield return Create($"Category_{name1}_Before_{name2}", source2 + " " + source1, source1 + " " + source2);
        }
        yield return Create(
            "AllMemberCategories",
            string.Join(" ", MemberKinds.Reverse().Select(member => member.Source)),
            string.Join(" ", MemberKinds.Select(member => member.Source))
        );
    }

    private static IEnumerable<DiagnosticTestCase> AccessibilityCases()
    {
        foreach (
            var (lower, higher) in new[]
            {
                ("private", "protected"),
                ("protected", "private protected"),
                ("protected", "protected internal"),
                ("private protected", "internal"),
                ("protected internal", "internal"),
                ("internal", "public"),
            }
        )
        {
            yield return Create(
                $"Accessibility_{higher}_Before_{lower}",
                $"{lower} void A() {{}} {higher} void Z() {{}}",
                $"{higher} void Z() {{}} {lower} void A() {{}}"
            );
        }
        foreach (
            var (name, lower, higher) in new[]
            {
                ("ConstantAccessibility", "private const int A = 1;", "public const int Z = 2;"),
                ("FieldAccessibility", "private int _a;", "public int Z;"),
                ("PropertyAccessibility", "private int A => 1;", "public int Z => 2;"),
                ("EventAccessibility", "private event System.Action A;", "public event System.Action Z;"),
                ("NestedTypeAccessibility", "private class A {}", "public class Z {}"),
                ("ConstructorAccessibilityBeforeArity", "private Example() {}", "public Example(int value) {}"),
                ("EqualAccessibilityUsesNames", "private protected void Z() {}", "protected internal void A() {}"),
            }
        )
        {
            yield return Create(name, lower + " " + higher, higher + " " + lower);
        }
    }

    private static IEnumerable<DiagnosticTestCase> ModifierCases()
    {
        foreach (
            var (name, lower, higher) in new[]
            {
                ("ReadonlyBeforeMutable", "private int _a;", "private readonly int _z;"),
                ("StaticBeforeReadonly", "private readonly int _a;", "private static int _z;"),
                ("StaticReadonlyBeforeStaticMutable", "private static int _a;", "private static readonly int _z;"),
                ("StaticMethod", "private void A() {}", "private static void Z() {}"),
                ("StaticProperty", "public int A => 1;", "public static int Z => 2;"),
                ("StaticEvent", "public event System.Action A;", "public static event System.Action Z;"),
                ("StaticNestedClass", "public class A {}", "public static class Z {}"),
                ("StaticConstructor", "private Example() {}", "static Example() {}"),
                ("AccessibilityBeforeStatic", "private static void A() {}", "public void Z() {}"),
                ("AccessibilityBeforeReadonly", "private readonly int _a;", "public int Z;"),
            }
        )
        {
            yield return Create(name, lower + " " + higher, higher + " " + lower);
        }
        yield return Create(
            "AllFieldModifiers",
            "private int _a; private readonly int _b; private static int _c; private static readonly int _d;",
            "private static readonly int _d; private static int _c; private readonly int _b; private int _a;"
        );
    }

    private static IEnumerable<DiagnosticTestCase> NameCases()
    {
        foreach (
            var (name, later, earlier) in new[]
            {
                ("ConstantNames", "private const int Z = 1;", "private const int A = 2;"),
                ("DelegateNames", "public delegate void Z();", "public delegate void A();"),
                ("OrdinalFieldNames", "private int _alpha;", "private int _Zeta;"),
                ("OrdinalMethodNames", "public void alpha() {}", "public void Zeta() {}"),
                ("EventNames", "public event System.Action Z;", "public event System.Action A;"),
                ("EnumNames", "public enum Z { Value }", "public enum A { Value }"),
                ("InterfaceNames", "public interface IZ {}", "public interface IA {}"),
                ("StructNames", "public struct Z {}", "public struct A {}"),
                ("NestedClassNames", "public class Z {}", "public class A {}"),
                ("RecordNames", "public record Z;", "public record A;"),
                ("IndexerReturnTypes", "public string this[int value] => \"\";", "public int this[string value] => 0;"),
                ("FirstVariableDeterminesFieldName", "private int _z, _a;", "private int _b;"),
            }
        )
        {
            yield return Create(name, later + " " + earlier, earlier + " " + later);
        }
    }

    private static IEnumerable<DiagnosticTestCase> TypeCases()
    {
        foreach (
            var declaration in new[]
            {
                "struct Example",
                "readonly struct Example",
                "ref struct Example",
                "record Example",
                "record struct Example",
                "static class Example",
                "class Example<T>",
                "partial class Example",
                "record Example(int Value)",
                "record struct Example(int Value)",
            }
        )
        {
            var modifiers = declaration.StartsWith("static", StringComparison.Ordinal) ? "static " : "";
            yield return Create(
                "Type_" + declaration,
                $"{modifiers}void Z() {{}} {modifiers}void A() {{}}",
                $"{modifiers}void A() {{}} {modifiers}void Z() {{}}",
                declaration
            );
        }
        yield return Create("InterfaceMethods", "void Z(); void A();", "void A(); void Z();", "interface Example");
        yield return new(
            "NestedClassMembers",
            "class Outer { class Example { void Z() {} void A() {} } }",
            "class Outer { class Example { void A() {} void Z() {} } }",
            "Example"
        );
    }

    private static IEnumerable<DiagnosticTestCase> StabilityCases()
    {
        yield return Create(
            "PreserveMethodOverloadOrder",
            "public void Z() {} public void Run(int value) {} public void Run() {} public void A() {}",
            "public void A() {} public void Run(int value) {} public void Run() {} public void Z() {}"
        );
        yield return Create(
            "PreservePropertyDeclarationOrder",
            "public void Run() {} public int Z => 1; public int A => 2;",
            "public int Z => 1; public int A => 2; public void Run() {}"
        );
        yield return Create(
            "PreserveConstructorOrderForEqualArity",
            "public Example(string value) {} public Example(int value) {} private int _value;",
            "private int _value; public Example(string value) {} public Example(int value) {}"
        );
        yield return Create(
            "PreserveOperatorDeclarationOrder",
            "public void Run() {} public static Example operator -(Example x, Example y) => x; public static Example operator +(Example x, Example y) => x;",
            "public static Example operator -(Example x, Example y) => x; public static Example operator +(Example x, Example y) => x; public void Run() {}"
        );
        yield return Create(
            "PreserveConversionDeclarationOrder",
            "public void Run() {} public static implicit operator long(Example value) => 0; public static implicit operator int(Example value) => 0;",
            "public static implicit operator long(Example value) => 0; public static implicit operator int(Example value) => 0; public void Run() {}"
        );

        const string contract = "interface IWorker { void Run(); } class Example : IWorker { ";
        yield return new(
            "ExplicitImplementationBeforePrivateMethod",
            contract + "private void A() {} void IWorker.Run() {} }",
            contract + "void IWorker.Run() {} private void A() {} }",
            "Example"
        );
        const string namedContract = "interface IWorker { void A(); } class Example : IWorker { ";
        yield return new(
            "ExplicitAndPublicMethodsUseNames",
            namedContract + "public void Z() {} void IWorker.A() {} }",
            namedContract + "void IWorker.A() {} public void Z() {} }",
            "Example"
        );
        const string twoMethods = "interface IWorker { void A(); void Z(); } class Example : IWorker { ";
        yield return new(
            "ExplicitImplementationNames",
            twoMethods + "void IWorker.Z() {} void IWorker.A() {} }",
            twoMethods + "void IWorker.A() {} void IWorker.Z() {} }",
            "Example"
        );
        yield return Create(
            "PreserveMemberComments",
            """
            /// <summary>Runs the operation.</summary>
            public void Run() {}
            // Stores the value.
            private int _value;
            """,
            """
            // Stores the value.
            private int _value;
            /// <summary>Runs the operation.</summary>
            public void Run() {}
            """
        );
    }

    private static IEnumerable<DiagnosticTestCase> CustomEventCases()
    {
        yield return Create(
            "CustomEventBeforeMethod",
            "public void Run() {} public event System.Action Changed { add {} remove {} }",
            "public event System.Action Changed { add {} remove {} } public void Run() {}"
        );
        yield return Create(
            "CustomEventNames",
            "public event System.Action Z { add {} remove {} } public event System.Action A { add {} remove {} }",
            "public event System.Action A { add {} remove {} } public event System.Action Z { add {} remove {} }"
        );
        yield return Create(
            "MixedEventDeclarations",
            "public event System.Action Z; public event System.Action A { add {} remove {} }",
            "public event System.Action A { add {} remove {} } public event System.Action Z;"
        );
        yield return Create(
            "StaticCustomEvent",
            "public event System.Action A { add {} remove {} } public static event System.Action Z { add {} remove {} }",
            "public static event System.Action Z { add {} remove {} } public event System.Action A { add {} remove {} }"
        );
        yield return new(
            "ExplicitEventImplementation",
            "interface IWorker { event System.Action Changed; } class Example : IWorker { private void Run() {} event System.Action IWorker.Changed { add {} remove {} } }",
            "interface IWorker { event System.Action Changed; } class Example : IWorker { event System.Action IWorker.Changed { add {} remove {} } private void Run() {} }",
            "Example"
        );
    }

    private static IEnumerable<DiagnosticTestCase> ExtensionBlockCases()
    {
        const string extension = "extension(string value) { public int LengthPlusOne() => value.Length + 1; }";
        foreach (var accessibility in new[] { "public", "internal", "private" })
        {
            var method = accessibility + " static void Run() {}";
            yield return Create(
                "ExtensionBeforeMethod_" + accessibility,
                method + " " + extension,
                extension + " " + method,
                "static class Example"
            );
        }
        yield return Create(
            "ExtensionAfterInterface",
            extension + " public interface IWorker {}",
            "public interface IWorker {} " + extension,
            "static class Example"
        );
        yield return Create(
            "ExtensionBeforeProperty",
            "public static int Value => 1; " + extension,
            extension + " public static int Value => 1;",
            "static class Example"
        );
        yield return Create(
            "PreserveExtensionBlockOrder",
            "extension(string value) { public int Z() => value.Length; } public static void Run() {} extension(int value) { public int A() => value; }",
            "extension(string value) { public int Z() => value.Length; } extension(int value) { public int A() => value; } public static void Run() {}",
            "static class Example"
        );
        yield return Create(
            "GenericExtensionReceiver",
            "public static void Run() {} extension<T>(System.Collections.Generic.IEnumerable<T> values) where T : class { public bool HasValues() => System.Linq.Enumerable.Any(values); }",
            "extension<T>(System.Collections.Generic.IEnumerable<T> values) where T : class { public bool HasValues() => System.Linq.Enumerable.Any(values); } public static void Run() {}",
            "static class Example"
        );
        yield return Create(
            "StaticExtensionMembers",
            "public static void Run() {} extension(string) { public static string EmptyText => \"\"; public static string CreateText() => \"text\"; }",
            "extension(string) { public static string EmptyText => \"\"; public static string CreateText() => \"text\"; } public static void Run() {}",
            "static class Example"
        );
        yield return Create(
            "ExtensionBeforeNestedType",
            "public class Nested {} " + extension,
            extension + " public class Nested {}",
            "static class Example"
        );
        yield return Create(
            "PreserveExtensionCommentsAndMembers",
            """
            public static void Run() {}
            // String extensions.
            extension(string value)
            {
                /// <summary>Gets the length.</summary>
                public int Size => value.Length;
                // Returns the receiver.
                public string Identity() => value;
            }
            """,
            """
            // String extensions.
            extension(string value)
            {
                /// <summary>Gets the length.</summary>
                public int Size => value.Length;
                // Returns the receiver.
                public string Identity() => value;
            }
            public static void Run() {}
            """,
            "static class Example"
        );
    }

    private static IEnumerable<DiagnosticTestCase> ExtensionMemberCases()
    {
        foreach (
            var (name, source, fixedSource) in new[]
            {
                (
                    "PropertyBeforeMethod",
                    "public void Run() {} public int Size => value.Length;",
                    "public int Size => value.Length; public void Run() {}"
                ),
                ("MethodNames", "public void Z() {} public void A() {}", "public void A() {} public void Z() {}"),
                (
                    "OrdinalMethodNames",
                    "public void alpha() {} public void Zeta() {}",
                    "public void Zeta() {} public void alpha() {}"
                ),
                (
                    "MethodAccessibility",
                    "private void A() {} public void Z() {}",
                    "public void Z() {} private void A() {}"
                ),
                (
                    "StaticMethod",
                    "public void A() {} public static void Z() {}",
                    "public static void Z() {} public void A() {}"
                ),
                (
                    "MethodAccessibilityBeforeStatic",
                    "private static void A() {} public void Z() {}",
                    "public void Z() {} private static void A() {}"
                ),
                (
                    "PropertyAccessibility",
                    "private int A => 1; public int Z => 2;",
                    "public int Z => 2; private int A => 1;"
                ),
                (
                    "StaticProperty",
                    "public int A => 1; public static int Z => 2;",
                    "public static int Z => 2; public int A => 1;"
                ),
                (
                    "PropertyAccessibilityBeforeStatic",
                    "private static int A => 1; public int Z => 2;",
                    "public int Z => 2; private static int A => 1;"
                ),
                (
                    "PreserveOverloadOrder",
                    "public void Z() {} public void Run(int x) {} public void Run() {} public void A() {}",
                    "public void A() {} public void Run(int x) {} public void Run() {} public void Z() {}"
                ),
                (
                    "PreservePropertyOrder",
                    "public void Run() {} public int Z => 1; public int A => 2;",
                    "public int Z => 1; public int A => 2; public void Run() {}"
                ),
            }
        )
        {
            yield return Create(
                "ExtensionMembers_" + name,
                "extension(string value) { " + source + " }",
                "extension(string value) { " + fixedSource + " }",
                "static class Example"
            );
        }
        yield return Create(
            "ReorderMultipleExtensionBlocksAndContainer",
            "public static int Value => 1; extension(string value) { public void Z() {} public void A() {} } extension(int value) { public void Run() {} public int Size => value; }",
            "extension(string value) { public void A() {} public void Z() {} } extension(int value) { public int Size => value; public void Run() {} } public static int Value => 1;",
            "static class Example"
        );
        yield return Create(
            "PreserveReorderedExtensionMemberComments",
            """
            extension(string value)
            {
                // Returns the receiver.
                public string Identity() => value;
                /// <summary>Gets the length.</summary>
                public int Size => value.Length;
            }
            """,
            """
            extension(string value)
            {
                /// <summary>Gets the length.</summary>
                public int Size => value.Length;
                // Returns the receiver.
                public string Identity() => value;
            }
            """,
            "static class Example"
        );
    }

    private static DiagnosticTestCase Create(
        string name,
        string members,
        string fixedMembers,
        string declaration = "class Example"
    ) => new(name, declaration + " { " + members + " }", declaration + " { " + fixedMembers + " }", "Example");
}
