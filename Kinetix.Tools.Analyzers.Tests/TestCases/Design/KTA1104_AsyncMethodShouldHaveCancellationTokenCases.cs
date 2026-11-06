using Microsoft.CodeAnalysis;

namespace Kinetix.Tools.Analyzers.Tests.TestCases.Design;

public static class KTA1104_AsyncMethodShouldHaveCancellationTokenCases
{
    private const string Helpers = """
        private static Task CancellableAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        private static Task WithParamsAsync(params CancellationToken[] cancellationTokens) => Task.CompletedTask;
        private static Task WithoutTokenAsync() => Task.CompletedTask;
        private static Task WithOverloadAsync(int value) => Task.CompletedTask;
        private static Task WithOverloadAsync(int value, CancellationToken cancellationToken) => Task.CompletedTask;
        private static Task DifferentOverloadAsync(int value) => Task.CompletedTask;
        private static Task DifferentOverloadAsync(string value, CancellationToken cancellationToken) => Task.CompletedTask;
        """;

    public static IEnumerable<object[]> All => Cases.Select(testCase => new object[] { testCase });

    public static string BuildSource(string method) =>
        $$"""
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;
            class Example
            {
                {{Helpers}}
                {{method}}
            }
            """;

    private static IEnumerable<DiagnosticTestCase> Cases
    {
        get
        {
            foreach (
                var (name, method, fixedMethod) in new[]
                {
                    (
                        "Task",
                        "Task RunAsync() { CancellableAsync(); return Task.CompletedTask; }",
                        "Task RunAsync(CancellationToken ct = default) { CancellableAsync(); return Task.CompletedTask; }"
                    ),
                    (
                        "GenericTask",
                        "Task<int> RunAsync() { CancellableAsync(); return Task.FromResult(1); }",
                        "Task<int> RunAsync(CancellationToken ct = default) { CancellableAsync(); return Task.FromResult(1); }"
                    ),
                    (
                        "ValueTask",
                        "ValueTask RunAsync() { CancellableAsync(); return ValueTask.CompletedTask; }",
                        "ValueTask RunAsync(CancellationToken ct = default) { CancellableAsync(); return ValueTask.CompletedTask; }"
                    ),
                    (
                        "GenericValueTask",
                        "ValueTask<int> RunAsync() { CancellableAsync(); return ValueTask.FromResult(1); }",
                        "ValueTask<int> RunAsync(CancellationToken ct = default) { CancellableAsync(); return ValueTask.FromResult(1); }"
                    ),
                    (
                        "AsyncEnumerable",
                        "async IAsyncEnumerable<int> RunAsync() { await CancellableAsync(); yield return 1; }",
                        "async IAsyncEnumerable<int> RunAsync(CancellationToken ct = default) { await CancellableAsync(); yield return 1; }"
                    ),
                    (
                        "EmptyParamsToken",
                        "Task RunAsync() { WithParamsAsync(); return Task.CompletedTask; }",
                        "Task RunAsync(CancellationToken ct = default) { WithParamsAsync(); return Task.CompletedTask; }"
                    ),
                    (
                        "CancellableOverload",
                        "Task RunAsync() { WithOverloadAsync(1); return Task.CompletedTask; }",
                        "Task RunAsync(CancellationToken ct = default) { WithOverloadAsync(1); return Task.CompletedTask; }"
                    ),
                    (
                        "TokenNotLast",
                        "Task RunAsync(CancellationToken cancellationToken, int value) { CancellableAsync(); return Task.CompletedTask; }",
                        "Task RunAsync(CancellationToken cancellationToken, int value, CancellationToken ct = default) { CancellableAsync(); return Task.CompletedTask; }"
                    ),
                    (
                        "Delay",
                        "public Task RunAsync() { return Task.Delay(1); }",
                        "public Task RunAsync(CancellationToken ct = default) { return Task.Delay(1); }"
                    ),
                    (
                        "ExistingParameter",
                        "public Task RunAsync(int value) { return Task.Delay(1); }",
                        "public Task RunAsync(int value, CancellationToken ct = default) { return Task.Delay(1); }"
                    ),
                }
            )
            {
                yield return Create(name, BuildSource(method), BuildSource(fixedMethod));
            }

            foreach (var separateDocument in new[] { false, true })
            {
                foreach (var threadingImported in new[] { false, true })
                {
                    var usings = threadingImported
                        ? "using System.Threading; using System.Threading.Tasks; "
                        : "using System.Threading.Tasks; ";
                    var fixedUsings = threadingImported ? usings : usings + "using System.Threading; ";
                    const string contract = "interface IExample { Task RunAsync(); }";
                    const string implementation =
                        "class Example : IExample { public Task RunAsync() { return Task.Delay(1); } }";
                    const string fixedContract =
                        "interface IExample { Task RunAsync(CancellationToken ct = default); }";
                    const string fixedImplementation =
                        "class Example : IExample { public Task RunAsync(CancellationToken ct = default) { return Task.Delay(1); } }";
                    yield return Create(
                        $"Contract_Separate={separateDocument}_Import={threadingImported}",
                        usings + (separateDocument ? "" : contract) + implementation,
                        fixedUsings + (separateDocument ? "" : fixedContract) + fixedImplementation
                    ) with
                    {
                        AdditionalDocuments = separateDocument
                            ? [new("IExample.cs", usings + contract, fixedUsings + fixedContract)]
                            : [],
                    };
                }
            }

            yield return Create(
                "MissingImport",
                "using System.Threading.Tasks; class Example { public Task RunAsync() { return Task.Delay(1); } }",
                "using System.Threading.Tasks; using System.Threading; class Example { public Task RunAsync(CancellationToken ct = default) { return Task.Delay(1); } }"
            );
            yield return Create(
                "ParameterDocumentation",
                """
                using System.Threading;
                using System.Threading.Tasks;
                class Example
                {
                    /// <summary>Runs the operation.</summary>
                    /// <param name="value">The value.</param>
                    public Task RunAsync(int value) { return Task.Delay(1); }
                }
                """,
                """
                using System.Threading;
                using System.Threading.Tasks;
                class Example
                {
                    /// <summary>Runs the operation.</summary>
                    /// <param name="value">The value.</param>
                    /// <param name="ct">CancellationToken.</param>
                    public Task RunAsync(int value, CancellationToken ct = default) { return Task.Delay(1); }
                }
                """
            );
        }
    }

    private static DiagnosticTestCase Create(string name, string source, string fixedSource) =>
        new(name, source, fixedSource, "RunAsync") { Severity = DiagnosticSeverity.Info };
}
