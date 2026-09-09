using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Tests.Common;

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
    public async Task OneTimeSetUp()
    {
        await Factory.InitializeAsync();
        
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

        // Sembrar módulo y rol de Administrador para que las llamadas con ValidateAccessAsync tengan permiso
        var module = new Module
        {
            Id = Guid.NewGuid(),
            Code = "PURCHASING",
            ModuleName = "Compras",
            IsActive = true
        };
        dbContext.Modules.Add(module);

        var adminRole = new Role
        {
            Id = Guid.NewGuid(),
            RoleName = "Administrador",
            Description = "Administrador de Compras",
            RoleType = RoleType.Administrator
        };
        dbContext.Roles.Add(adminRole);

        var userProfiles = await dbContext.Profiles
            .Where(p => p.UserId == DefaultUserId)
            .ToListAsync();

        foreach (var profile in userProfiles)
        {
            dbContext.UserModuleRoles.Add(new UserModuleRoles
            {
                Id = Guid.NewGuid(),
                UserProfileId = profile.Id,
                ModuleId = module.Id,
                RoleId = adminRole.Id,
                ModuleCode = module.Code,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync();

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

    protected static JsonSerializerOptions SnakeCaseJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return options;
    }
}
