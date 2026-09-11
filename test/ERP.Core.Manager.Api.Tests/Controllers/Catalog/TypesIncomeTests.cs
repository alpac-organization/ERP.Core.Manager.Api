using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class TypesIncomeTests : IntegrationTestBase
    {
        private static string TypesIncomePath(Guid companyId) =>
            $"/api/v1/companies/{companyId}/types-income";

        private async Task SeedTypesIncomeAsync(string code, string title, bool isActive)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ErpDbContext>();

            dbContext.TypesIncomes.Add(new TypesIncome
            {
                Id = Guid.NewGuid(),
                IncomeCode = code,
                IncomeTitle = title,
                IncomeDescription = $"{title} (test)",
                IsActive = isActive
            });

            await dbContext.SaveChangesAsync();
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task GetTypesIncome_ReturnsActiveTypesForCompany(Guid companyId)
        {
            await SeedTypesIncomeAsync("ALW_MEAL", "Viático de alimentación", isActive: true);

            var response = await SendAsync(HttpMethod.Get, TypesIncomePath(companyId));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = await response.Content.ReadAsStringAsync();
            var types = JsonSerializer.Deserialize<List<TypesIncomeDto>>(json, SnakeCaseJsonOptions());

            Assert.That(types, Is.Not.Null);
            Assert.That(types!.Any(t => t.IncomeCode == "ALW_MEAL" && t.IncomeTitle == "Viático de alimentación"), Is.True,
                "El tipo de ingreso activo debe estar disponible para asignar viáticos.");
            Assert.That(types!.All(t => t.TypeIncomeId != Guid.Empty), Is.True);
        }

        [TestCaseSource(typeof(TestCompanies), nameof(TestCompanies.All))]
        public async Task GetTypesIncome_ExcludesInactiveTypes(Guid companyId)
        {
            await SeedTypesIncomeAsync("ALW_TRANSPORT", "Viático de transporte", isActive: true);
            await SeedTypesIncomeAsync("ALW_INACTIVE", "Viático inactivo", isActive: false);

            var response = await SendAsync(HttpMethod.Get, TypesIncomePath(companyId));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = await response.Content.ReadAsStringAsync();
            var types = JsonSerializer.Deserialize<List<TypesIncomeDto>>(json, SnakeCaseJsonOptions());

            Assert.That(types, Is.Not.Null);
            Assert.That(types!.Any(t => t.IncomeCode == "ALW_TRANSPORT"), Is.True);
            Assert.That(types!.Any(t => t.IncomeCode == "ALW_INACTIVE"), Is.False,
                "Los tipos de ingreso inactivos no deben listarse.");
        }
    }
}
