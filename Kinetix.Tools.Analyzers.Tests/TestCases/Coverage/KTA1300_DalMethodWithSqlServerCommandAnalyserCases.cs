using Microsoft.CodeAnalysis;

namespace Kinetix.Tools.Analyzers.Tests.TestCases.Coverage;

public static class KTA1300_DalMethodWithSqlServerCommandAnalyserCases
{
    private const string TestSupport = """
        namespace Sample.Business.Common.Test
        {
            public class DalTest
            {
                protected static ServiceProvider Provider { get; } = new ServiceProvider();
                public void CheckDalSyntax<T>(System.Action<T> action) {}
            }
            public class ServiceProvider { public T GetService<T>() => default; }
            public static class Dum { public static int Id => 1; }
        }
        """;

    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            foreach (var className in new[] { "DalExample", "AbstractDal" })
            {
                foreach (var invocation in new[] { "GetSqlCommand", "GetBroker" })
                {
                    foreach (var actionIndex in new[] { 0, 1 })
                    {
                        yield return Create(
                            $"{className}_{invocation}_Action={actionIndex}",
                            className,
                            invocation,
                            actionIndex,
                            hasParameter: false
                        );
                    }
                }
            }
            foreach (var actionIndex in new[] { 0, 1 })
            {
                yield return Create("DummyParameter_Action=" + actionIndex, "DalExample", "GetSqlCommand", actionIndex);
            }

            var generation = Create("Base", "DalExample", "GetSqlCommand", 0);
            yield return generation with
            {
                Name = "MissingTestProject",
                ActionCount = 0,
                TestProjectSupport = null,
                GeneratedDocument = null,
            };
            var privateSource = generation.Source.Replace("public void Load", "private void Load");
            yield return generation with
            {
                Name = "PrivateMethod",
                Source = privateSource,
                FixedSource = privateSource,
                ActionCount = 0,
                GeneratedDocument = null,
            };
            yield return generation with
            {
                Name = "ExistingTest",
                ActionCount = 0,
                GeneratedDocument = null,
                TestProjectDocuments = [new("LoadTest.cs", "", "") { Folders = ["DalExampleTest"] }],
            };
        }
    }

    private static DiagnosticTestCase Create(
        string name,
        string className,
        string invocation,
        int actionIndex,
        bool hasParameter = true
    )
    {
        var declaration =
            className == "DalExample" ? "[RegisterImpl] public class DalExample" : "public class AbstractDal";
        var signature = hasParameter ? "void Load(int id)" : "object Load()";
        var body = hasParameter ? invocation + "();" : "return " + invocation + "();";
        var source =
            ServiceTestSources.Annotations
            + $$"""
                namespace Sample.Dal.Implementation
                {
                    {{declaration}}
                    {
                        private object {{invocation}}() => null;
                        public {{signature}} { {{body}} }
                    }
                }
                """;
        var arguments = hasParameter ? "Dum.Id" : "";
        var call =
            actionIndex == 0
                ? $"this.CheckDalSyntax<{className}>(dal => dal.Load({arguments}));"
                : $"Provider.GetService<{className}>().Load({arguments});";
        var expectedTest = $$"""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Sample.Business.Common.Test;
            using Sample.Dal.Implementation;
            namespace Sample.Dal.Implementation.Test.{{className}}Test
            {
                [TestClass]
                public class LoadTest : DalTest
                {
                    [TestMethod]
                    public void Check_Load_Ok()
                    {
                        // Act
                        {{call}}
                    }
                }
            }
            """;
        return new(name, source, source, "Load")
        {
            ProjectName = "Sample.Dal.Implementation",
            Severity = DiagnosticSeverity.Hidden,
            ActionCount = 2,
            ActionIndex = actionIndex,
            DiagnosticRemains = true,
            TestProjectSupport = TestSupport,
            GeneratedDocument = new("LoadTest.cs", expectedTest, [className + "Test"]),
        };
    }
}
