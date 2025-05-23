using Kinetix.DataAccess.Sql.Postgres;
using Kinetix.Monitoring.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Elastic;
using Kinetix.User;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// TODO adapater nom du projet
namespace MonProjet.Business.Common.Test
{
    [TestClass]
    public static class TestUtil
    {
        public static ServiceProvider Provider { get; set; }

        public static IServiceCollection GetServiceCollection(TestContext testContext)
        {
            return new ServiceCollection()
                .AddLogging()
                .AddPostgres(c => c
                    .AddDefaultConnectionString($"Server={(string)testContext.Properties["DatabaseServer"]};Database={(string)testContext.Properties["DatabaseName"]};User Id={(string)testContext.Properties["DatabaseLogin"]};Password={(string)testContext.Properties["DatabasePassword"]};Port=5432;Ssl Mode=VerifyFull")
                    .WithDefaultCommandTimeout(0))
                .AddSingleton(testContext);               
        }
    }
}
