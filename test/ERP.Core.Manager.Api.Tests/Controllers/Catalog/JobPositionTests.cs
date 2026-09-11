using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class JobPositionTests : IntegrationTestBase
    {
        private const string ModuleCode = PayrollModuleCode;

        private static string JobPositionsPath(Guid companyId) =>
            $"/api/v1/companies/{companyId}/modules/{ModuleCode}/job-positions";

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterJobPosition_PersistsAndReturnsItForCompany(Guid companyId)
        {
            var name = $"Cargo {companyId.ToString()[..4]}";

            var response = await SendAsync(HttpMethod.Post, JobPositionsPath(companyId), new RegisterJobPositionCommand
            {
                JobPositionName = name,
                Description = "Cargo creado por test de integración"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var getResponse = await SendAsync(HttpMethod.Get, JobPositionsPath(companyId));
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = await getResponse.Content.ReadAsStringAsync();
            var positions = JsonSerializer.Deserialize<List<JobPositionDto>>(json, SnakeCaseJsonOptions());

            Assert.That(positions, Is.Not.Null);
            Assert.That(positions!.Any(p => p.CompanyId == companyId && p.JobPositionName == name), Is.True,
                "El cargo registrado debe aparecer en el listado de su compañía.");
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteJobPosition_SoftDeletesForCompany(Guid companyId)
        {
            var name = $"Eliminar Cargo {companyId.ToString()[..4]}";

            var create = await SendAsync(HttpMethod.Post, JobPositionsPath(companyId), new RegisterJobPositionCommand
            {
                JobPositionName = name,
                Description = "Cargo a eliminar"
            });
            Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var jobPosition = await UnitOfWork.JobPositions.Entities
                .AsNoTracking()
                .Where(j => j.CompanyId == companyId && j.JobPositionName == name)
                .OrderByDescending(j => j.CreatedAt)
                .FirstAsync();

            var response = await SendAsync(HttpMethod.Delete, $"{JobPositionsPath(companyId)}/{jobPosition.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

            var deleted = await UnitOfWork.JobPositions.Entities.AsNoTracking().FirstAsync(j => j.Id == jobPosition.Id);
            Assert.That(deleted.IsActive, Is.False);
            Assert.That(deleted.DeletedAt, Is.Not.Null);
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterJobPosition_WithoutName_ReturnsBadRequest(Guid companyId)
        {
            var response = await SendAsync(HttpMethod.Post, JobPositionsPath(companyId), new RegisterJobPositionCommand
            {
                JobPositionName = string.Empty,
                Description = "Sin nombre"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task RegisterJobPosition_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Post, JobPositionsPath(companyId), new RegisterJobPositionCommand
            {
                JobPositionName = "Cargo sin permiso",
                Description = "No debería registrarse"
            });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task DeleteJobPosition_AsNonAdmin_ReturnsBadRequest(Guid companyId)
        {
            var name = $"Cargo Protegido {companyId.ToString()[..4]}";

            await SendAsync(HttpMethod.Post, JobPositionsPath(companyId), new RegisterJobPositionCommand
            {
                JobPositionName = name,
                Description = "Cargo que no debe poder eliminarse sin admin"
            });

            var jobPosition = await UnitOfWork.JobPositions.Entities
                .AsNoTracking()
                .Where(j => j.CompanyId == companyId && j.JobPositionName == name)
                .OrderByDescending(j => j.CreatedAt)
                .FirstAsync();

            await DemoteDefaultUserToRoleAsync(RoleType.Operator);

            var response = await SendAsync(HttpMethod.Delete, $"{JobPositionsPath(companyId)}/{jobPosition.Id}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var stillActive = await UnitOfWork.JobPositions.Entities.AsNoTracking().FirstAsync(j => j.Id == jobPosition.Id);
            Assert.That(stillActive.IsActive, Is.True);
        }
    }
}
