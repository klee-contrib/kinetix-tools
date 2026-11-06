namespace Kinetix.Tools.Analyzers.Tests;

internal static class ServiceTestSources
{
    public const string Annotations = """
        using Kinetix.Services.Annotations;

        namespace Kinetix.Services.Annotations
        {
            public sealed class RegisterContractAttribute : System.Attribute { }
            public sealed class RegisterImplAttribute : System.Attribute { }
        }

        """;

    public const string Service =
        Annotations
        + """
            [RegisterContract]
            interface IServiceExample { void Run(); }

            [RegisterImpl]
            class ServiceExample : IServiceExample
            {
                public void Run() { }
            }

            """;
}
