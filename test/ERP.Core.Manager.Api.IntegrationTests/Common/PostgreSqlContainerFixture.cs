namespace ERP.Core.Manager.Api.IntegrationTests;

/// <summary>
/// [SetUpFixture] de NUnit: clase delgada que administra exclusivamente la fábrica global
/// (<see cref="Factory"/>). Todo el ciclo de vida del contenedor PostgreSQL y el reset de datos
/// viven dentro de <see cref="CustomWebApplicationFactory"/>.
/// </summary>
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
