using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Tools.TestUtils.Helpers;

public static class SharedState
{
    public static ServiceProvider Provider { get; set; }
}
