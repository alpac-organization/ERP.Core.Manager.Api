using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Testing.Seeding;

namespace ERP.Core.Manager.Api.IntegrationTests.Common;

/// <summary>
/// Base para los tests de integración. Solo reemplaza la cadena de conexión por la del contenedor
/// (vía <see cref="CustomWebApplicationFactory"/>), siembra el escenario real (empresas/usuarios de
/// ERP.Core.Testing) y deja lista la DI de la app para resolver <see cref="IUnitOfWork"/> y demás.
/// </summary>
[TestFixture]
public abstract class IntegrationTestBase
{
    protected HttpClient Client = null!;
    protected IServiceProvider Services = null!;
    protected Guid DefaultUserId { get; private set; }

    /// <summary>Empresa por defecto (ALPAC) — id determinista definido en la semilla.</summary>
    protected static Guid DefaultCompanyId => Guid.Parse("11111111-1111-1111-1111-111111111111");

    protected IUnitOfWork UnitOfWork => Services.GetRequiredService<IUnitOfWork>();

    private static CustomWebApplicationFactory Factory => PostgreSqlContainerFixture.Factory;


    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (!Factory.IsDockerAvailable)
        {
            TestContext.Out.WriteLine($"[Testcontainers] No disponible. Razón: {Factory.UnavailableReason}");
            return;
        }

        Client = Factory.CreateClient();
        Client.DefaultRequestHeaders.Add("x-api-key", CustomWebApplicationFactory.ApiKey);
        Client.DefaultRequestHeaders.Add("x-device-name", "Test-Device");

        Services = Factory.Services;
    }

    [SetUp]
    public async Task SetUp()
    {
        if (!Factory.IsDockerAvailable)
        {
            Assert.Ignore($"No se pudo levantar el contenedor de pruebas, se omiten los tests de integración. Detalle: {Factory.UnavailableReason}");
        }

        await Factory.ResetDatabaseAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        await ErpDatabaseSeeder.SeedAsync(dbContext, ErpSeedDataFactory.CreateScenario());

        // Usuario del área "Tecnología de la Información" de ALPAC: tiene perfil en las 5 empresas,
        // por lo que el token por defecto sirve para autenticar en cualquiera de ellas.
        DefaultUserId = await dbContext.Users
            .Where(u => u.AreaId == ErpSeedDataFactory.AlpacAreaTiId)
            .Select(u => u.Id)
            .FirstAsync();

        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            TestAuthHelper.CreateBearerToken(CustomWebApplicationFactory.JwtKey, DefaultUserId));
    }

    protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, Guid? asUser = null)
    {
        var request = new HttpRequestMessage(method, path);

        if (asUser.HasValue)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                TestAuthHelper.CreateBearerToken(CustomWebApplicationFactory.JwtKey, asUser.Value));
        }

        if (body is not null)
        {
            request.Content = new StringContent(
                JsonSerializer.Serialize(body, SnakeCaseJsonOptions()),
                Encoding.UTF8,
                "application/json");
        }

        return await Client.SendAsync(request);
    }

    protected static JsonSerializerOptions SnakeCaseJsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };
}
