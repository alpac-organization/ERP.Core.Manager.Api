using ERP.Core.Manager.Api.Tests.Common;

namespace ERP.Core.Manager.Api.Tests;

[SetUpFixture]
public class PostgreSqlContainerFixture
{
    public static CustomWebApplicationFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Factory = new CustomWebApplicationFactory();
        await Factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Factory.DisposeAsync();
    }
}
