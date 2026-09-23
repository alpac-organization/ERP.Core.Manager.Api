using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;

using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Testing.Seeding;
using ERP.Core.Manager.Api.Tests.Common.Utils;

namespace ERP.Core.Manager.Api.Tests.Common
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        private IServiceScope _scope = null!;

        protected HttpClient _client = null!;
        protected IUnitOfWork _unitOfWork = null!;
        protected static CustomWebApplicationFactory Factory => PostgreSqlContainerFixture.Factory;

        // Backward compatibility
        protected HttpClient Client => _client;
        protected IServiceProvider Services => Factory.Services;
        protected IUnitOfWork UnitOfWork => _unitOfWork;
        protected Guid DefaultUserId { get; private set; }
        protected static Guid DefaultCompanyId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string PayrollModuleCode = "NOMINA";

        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        [SetUp]
        public async Task SetUp()
        {
            _client = PostgreSqlContainerFixture.Factory.CreateClient();

            //Limpiamos la base de datos para el uso de ella
            await Factory.ResetDatabase();
            await Factory.SeedDatabase();

            //Definir los servicios
            _scope = Factory.Services.CreateScope();
            _unitOfWork = ServiceProviderServiceExtensions.GetRequiredService<IUnitOfWork>(_scope.ServiceProvider);

            // Backward compatibility: find a user from ALPAC company
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var alpacCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            DefaultUserId = await dbContext.Profiles
                .Where(p => p.CompanyId == alpacCompanyId && p.IsActive)
                .Select(p => p.UserId)
                .FirstAsync();

            await GrantModuleAccessAsync(PayrollModuleCode, RoleType.Administrator);
            await GrantModuleAccessAsync("PURCHASING", RoleType.Administrator);

            Client.DefaultRequestHeaders.Add("x-api-key", EnvironmentManager.ApiKey);
            Client.DefaultRequestHeaders.Add("x-device-name", "Test-Device");
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                AuthManager.GenerateJwtToken(EnvironmentManager.JwtKey, DefaultUserId));
        }

        [TearDown]
        public void TearDown()
        {
            _scope.Dispose();
            _client.Dispose();
        }

        // Backward compatibility method
        protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null, Guid? asUser = null)
        {
            var request = new HttpRequestMessage(method, path);

            if (asUser.HasValue)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    AuthManager.GenerateJwtToken(EnvironmentManager.JwtKey, asUser.Value));
            }
            else
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    AuthManager.GenerateJwtToken(EnvironmentManager.JwtKey, DefaultUserId));
            }

            if (body is not null)
            {
                var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            return await _client.SendAsync(request);
        }

        // New method matching Warehouse.Api pattern
        protected async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string pathUrl, string BearerToken, object? body = null)
        {
            var request = new HttpRequestMessage(method, pathUrl);

            request.Headers.Add("X-Api-Key", EnvironmentManager.ApiKey);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer", BearerToken
            );

            if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
            {
                var jsonBody = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            }

            return await _client.SendAsync(request);
        }

        // Backward compatibility
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

        // Backward compatibility
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

        // Backward compatibility
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
    }
}