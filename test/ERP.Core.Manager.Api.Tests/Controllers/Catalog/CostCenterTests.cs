using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class CostCenterTests : IntegrationTestBase
    {
        private const string ModuleCode = PayrollModuleCode;

        private static string CostCentersPath(Guid companyId, Guid areaId) => $"/api/v1/companies/{companyId}/modules/{ModuleCode}/areas/{areaId}/cost-centers";

        private async Task<Guid> GetSeededAreaIdAsync(Guid companyId)
        {
            var area = await UnitOfWork.WorkAreas.Entities
                .AsNoTracking()
                .Where(w => w.CompanyId == companyId)
                .OrderBy(w => w.WorkAreaCode)
                .FirstAsync();

            return area.Id;
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterCostCenter_PersistsAndReturnsItForCompany(Guid companyId)
        {
            var areaId = await GetSeededAreaIdAsync(companyId);
            var name = $"Centro {companyId.ToString()[..4]}";

            var response = await SendAsync(HttpMethod.Post, CostCentersPath(companyId, areaId), new RegisterCostCenterCommand
            {
                CoilCode = 101,
                CostCenterName = name,
                Description = "Centro de costo creado por test de integración"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var getResponse = await SendAsync(HttpMethod.Get, CostCentersPath(companyId, areaId));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = await getResponse.Content.ReadAsStringAsync();
            var costCenters = JsonSerializer.Deserialize<List<CostCenterDto>>(json, SnakeCaseJsonOptions());

            Assert.That(costCenters, Is.Not.Null);
            Assert.That(costCenters!.Any(c => c.CostCenterName == name && c.CoilCode == 101), Is.True,
                "El centro de costo registrado debe aparecer en el listado del área.");
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteCostCenter_SoftDeletesForCompany(Guid companyId)
        {
            var areaId = await GetSeededAreaIdAsync(companyId);
            var name = $"Eliminar Centro {companyId.ToString()[..4]}";

            var create = await SendAsync(HttpMethod.Post, CostCentersPath(companyId, areaId), new RegisterCostCenterCommand
            {
                CoilCode = 202,
                CostCenterName = name,
                Description = "Centro de costo a eliminar"
            });
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var costCenter = await UnitOfWork.CostCenters.Entities
                .AsNoTracking()
                .Where(c => c.WorkAreaId == areaId && c.CostCenterName == name)
                .OrderByDescending(c => c.CostCenterCode)
                .FirstAsync();

            var response = await SendAsync(HttpMethod.Delete, $"{CostCentersPath(companyId, areaId)}/{costCenter.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var deleted = await UnitOfWork.CostCenters.Entities.AsNoTracking().FirstAsync(c => c.Id == costCenter.Id);
            Assert.That(deleted.IsActive, Is.False);
            Assert.That(deleted.DeletedAt, Is.Not.Null);
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterCostCenter_WithoutName_ReturnsBadRequest(Guid companyId)
        {
            var areaId = await GetSeededAreaIdAsync(companyId);

            var response = await SendAsync(HttpMethod.Post, CostCentersPath(companyId, areaId), new RegisterCostCenterCommand
            {
                CoilCode = 303,
                CostCenterName = string.Empty,
                Description = "Sin nombre"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterCostCenter_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            var areaId = await GetSeededAreaIdAsync(companyId);
            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Post, CostCentersPath(companyId, areaId), new RegisterCostCenterCommand
            {
                CoilCode = 404,
                CostCenterName = "Centro sin permiso",
                Description = "No debería registrarse"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteCostCenter_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            var areaId = await GetSeededAreaIdAsync(companyId);
            var name = $"Centro Protegido {companyId.ToString()[..4]}";

            await SendAsync(HttpMethod.Post, CostCentersPath(companyId, areaId), new RegisterCostCenterCommand
            {
                CoilCode = 505,
                CostCenterName = name,
                Description = "Centro que no debe poder eliminarse sin admin"
            });

            var costCenter = await UnitOfWork.CostCenters.Entities
                .AsNoTracking()
                .Where(c => c.WorkAreaId == areaId && c.CostCenterName == name)
                .OrderByDescending(c => c.CostCenterCode)
                .FirstAsync();

            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Delete, $"{CostCentersPath(companyId, areaId)}/{costCenter.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var stillActive = await UnitOfWork.CostCenters.Entities.AsNoTracking().FirstAsync(c => c.Id == costCenter.Id);
            Assert.That(stillActive.IsActive, Is.True);
        }
    }
}
