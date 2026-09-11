using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class WorkAreaTests : IntegrationTestBase
    {
        private const string ModuleCode = PayrollModuleCode;
        private static string AreasPath(Guid companyId) => $"/api/v1/companies/{companyId}/modules/{ModuleCode}/areas";


        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterWorkArea_PersistsAndReturnsItForCompany(Guid companyId)
        {
            var name = $"Area {companyId.ToString()[..4]}";

            var response = await SendAsync(HttpMethod.Post, AreasPath(companyId), new RegisterWorkAreaCommand
            {
                WorkAreaName = name,
                Description = "Area creada por test de integración"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var getResponse = await SendAsync(HttpMethod.Get, AreasPath(companyId));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = await getResponse.Content.ReadAsStringAsync();
            var areas = JsonSerializer.Deserialize<List<WorkAreaDto>>(json, SnakeCaseJsonOptions());

            Assert.That(areas, Is.Not.Null);
            Assert.That(areas!.Any(a => a.CompanyId == companyId && a.WorkAreaName == name), Is.True,
                "El área registrada debe aparecer en el listado de su compañía.");
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteWorkArea_SoftDeletesForCompany(Guid companyId)
        {
            var name = $"Eliminar {companyId.ToString()[..4]}";

            var create = await SendAsync(HttpMethod.Post, AreasPath(companyId), new RegisterWorkAreaCommand
            {
                WorkAreaName = name,
                Description = "Area a eliminar"
            });
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var area = await UnitOfWork.WorkAreas.Entities
                .AsNoTracking()
                .Where(w => w.CompanyId == companyId && w.WorkAreaName == name)
                .OrderByDescending(w => w.WorkAreaCode)
                .FirstAsync();

            var response = await SendAsync(HttpMethod.Delete, $"{AreasPath(companyId)}/{area.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var deleted = await UnitOfWork.WorkAreas.Entities.AsNoTracking().FirstAsync(w => w.Id == area.Id);
            Assert.That(deleted.IsActive, Is.False);
            Assert.That(deleted.DeletedAt, Is.Not.Null);
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterWorkArea_WithoutName_ReturnsBadRequest(Guid companyId)
        {
            var response = await SendAsync(HttpMethod.Post, AreasPath(companyId), new RegisterWorkAreaCommand
            {
                WorkAreaName = string.Empty,
                Description = "Sin nombre"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterWorkArea_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Post, AreasPath(companyId), new RegisterWorkAreaCommand
            {
                WorkAreaName = "Area sin permiso",
                Description = "No debería registrarse"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteWorkArea_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            var name = $"Protegida {companyId.ToString()[..4]}";

            await SendAsync(HttpMethod.Post, AreasPath(companyId), new RegisterWorkAreaCommand
            {
                WorkAreaName = name,
                Description = "Area que no debe poder eliminarse sin admin"
            });

            var area = await UnitOfWork.WorkAreas.Entities
                .AsNoTracking()
                .Where(w => w.CompanyId == companyId && w.WorkAreaName == name)
                .OrderByDescending(w => w.WorkAreaCode)
                .FirstAsync();

            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Delete, $"{AreasPath(companyId)}/{area.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var stillActive = await UnitOfWork.WorkAreas.Entities.AsNoTracking().FirstAsync(w => w.Id == area.Id);
            Assert.That(stillActive.IsActive, Is.True);
        }
    }
}
