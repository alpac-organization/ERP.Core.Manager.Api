using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Tests.Common
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        protected HttpClient Client = null!;
        protected IServiceProvider Services = null!;
        protected Guid DefaultUserId { get; private set; }

        protected static Guid DefaultCompanyId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string PayrollModuleCode = "NOMINA";

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

            DefaultUserId = await dbContext.Users
                .Where(u => u.AreaId == ErpSeedDataFactory.AlpacAreaTiId)
                .Select(u => u.Id)
                .FirstAsync();

            await GrantModuleAccessAsync(PayrollModuleCode, RoleType.Administrator);
            await GrantModuleAccessAsync("PURCHASING", RoleType.Administrator);

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

        protected async Task<bool> GrantModuleAccessAsync(string moduleCode, RoleType roleType = RoleType.Administrator, Guid? userId = null)
        {
            var targetUserId = userId ?? DefaultUserId;

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var module = await dbContext.Modules.FirstOrDefaultAsync(m => m.Code == moduleCode);
            if (module is null)
            {
                module = new Module
                {
                    Id = Guid.NewGuid(),
                    Code = moduleCode,
                    ModuleName = moduleCode,
                    IsActive = true
                };
                dbContext.Modules.Add(module);
            }

            var role = new Role
            {
                Id = Guid.NewGuid(),
                RoleName = $"{roleType}-{moduleCode}",
                Description = $"Rol de prueba ({roleType}) para el módulo {moduleCode}",
                RoleType = roleType
            };
            dbContext.Roles.Add(role);

            var profiles = await dbContext.Profiles
                .Where(p => p.UserId == targetUserId)
                .ToListAsync();

            foreach (var profile in profiles)
            {
                dbContext.UserModuleRoles.Add(new UserModuleRoles
                {
                    Id = Guid.NewGuid(),
                    UserProfileId = profile.Id,
                    ModuleId = module.Id,
                    RoleId = role.Id,
                    ModuleCode = module.Code,
                    IsActive = true
                });
            }

            await dbContext.SaveChangesAsync();

            return await HasModuleAccessAsync(targetUserId, moduleCode);
        }

        protected async Task<bool> HasModuleAccessAsync(Guid userId, string moduleCode, Guid? companyId = null)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var query =
                from userModuleRole in dbContext.UserModuleRoles
                join profile in dbContext.Profiles on userModuleRole.UserProfileId equals profile.Id
                where userModuleRole.ModuleCode == moduleCode
                    && userModuleRole.IsActive
                    && profile.UserId == userId
                select profile;

            if (companyId.HasValue)
            {
                query = query.Where(profile => profile.CompanyId == companyId.Value);
            }

            return await query.AnyAsync();
        }

        protected async Task DemoteDefaultUserToRoleAsync(RoleType roleType)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            var roles = await dbContext.Roles.ToListAsync();
            foreach (var role in roles)
            {
                role.RoleType = roleType;
            }

            await dbContext.SaveChangesAsync();
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

}
