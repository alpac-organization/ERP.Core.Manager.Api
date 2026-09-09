using System.Net;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using NUnit.Framework;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Tests.Common;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class SupplierBankAccountTests : IntegrationTestBase
    {
        private const string ModuleCode = "PURCHASING";

        private async Task<(Guid CompanyId, Guid SupplierId)> CreateSupplierAsync(string legalName, string ruc, bool includeInitialAccount = true)
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = legalName,
                CommercialName = legalName,
                IdentificationNumber = ruc,
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Managua",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                },
                BankAccounts = includeInitialAccount
                    ? new List<SupplierBankAccountCommand>
                    {
                        new()
                        {
                            BankName = "BAC Credomatic",
                            AccountNumber = "100200300",
                            AccountType = BankAccountType.Checking,
                            Currency = Currency.USD,
                            AccountHolderName = legalName,
                            IsPrimary = true
                        }
                    }
                    : new List<SupplierBankAccountCommand>()
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegisterSupplierDto>(json, SnakeCaseJsonOptions());

            return (company.Id, result!.SupplierId);
        }

        [Test]
        public async Task AddSupplierBankAccount_HappyPath_FirstAccountAutomaticallyBecomesPrimary()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Sin Cuentas S.A.", "0010909260100A", includeInitialAccount: false);

            var addAccountCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Banco LAFISE",
                AccountNumber = "5544332211",
                AccountType = BankAccountType.Savings,
                Currency = Currency.NIO,
                AccountHolderName = "Proveedor Sin Cuentas S.A.",
                IsPrimary = false // Aunque sea false, como es la primera, debe convertirse en true automáticamente
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addAccountCmd);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var accountDto = JsonSerializer.Deserialize<SupplierBankAccountDto>(json, SnakeCaseJsonOptions());

            accountDto.Should().NotBeNull();
            accountDto!.BankName.Should().Be("Banco LAFISE");
            accountDto.AccountNumber.Should().Be("5544332211");
            accountDto.IsPrimary.Should().BeTrue();

            var accountInDb = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstOrDefaultAsync(b => b.AccountNumber == "5544332211");

            accountInDb.Should().NotBeNull();
            accountInDb!.IsPrimary.Should().BeTrue();
        }

        [Test]
        public async Task AddSupplierBankAccount_WhenMarkedPrimary_DemotesExistingPrimary()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Con Cuentas S.A.", "0010909260101B", includeInitialAccount: true);

            var addSecondAccountCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Banpro",
                AccountNumber = "7788990011",
                AccountType = BankAccountType.Checking,
                Currency = Currency.USD,
                AccountHolderName = "Proveedor Con Cuentas S.A.",
                IsPrimary = true // Marcada como principal -> debe desmarcar la anterior
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addSecondAccountCmd);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var accountsInDb = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .ToListAsync();

            accountsInDb.Should().HaveCount(2);

            var firstAccount = accountsInDb.First(a => a.AccountNumber == "100200300");
            var secondAccount = accountsInDb.First(a => a.AccountNumber == "7788990011");

            firstAccount.IsPrimary.Should().BeFalse();
            secondAccount.IsPrimary.Should().BeTrue();
        }

        [Test]
        public async Task AddSupplierBankAccount_WhenSupplierNotFound_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");
            var nonExistentSupplierId = Guid.NewGuid();

            var addAccountCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Banco Fantasma",
                AccountNumber = "0000000000",
                AccountType = BankAccountType.Savings,
                Currency = Currency.USD,
                AccountHolderName = "Nadie",
                IsPrimary = true
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{nonExistentSupplierId}/bank-accounts",
                addAccountCmd);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddSupplierBankAccount_WhenRequiredFieldsMissing_ReturnsBadRequest()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Incompleto S.A.", "0010909260102C", includeInitialAccount: false);

            var invalidCmd = new AddSupplierBankAccountCommand
            {
                BankName = "", // Vacío
                AccountNumber = "", // Vacío
                AccountHolderName = "" // Vacío
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                invalidCmd);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task UpdateSupplierBankAccount_HappyPath_UpdatesAccountFields()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Modificable S.A.", "0010909260103D", includeInitialAccount: true);

            var account = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync();

            var updateCmd = new UpdateSupplierBankAccountCommand
            {
                BankName = "BAC Credomatic Actualizado",
                AccountNumber = "999888777",
                Currency = Currency.NIO
            };

            var response = await SendAsync(
                HttpMethod.Patch,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{account.Id}",
                updateCmd);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedAccount = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(b => b.Id == account.Id);

            updatedAccount.BankName.Should().Be("BAC Credomatic Actualizado");
            updatedAccount.AccountNumber.Should().Be("999888777");
            updatedAccount.Currency.Should().Be(Currency.NIO);
        }

        [Test]
        public async Task UpdateSupplierBankAccount_TogglePrimary_DemotesOtherAccounts()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Toggle S.A.", "0010909260104E", includeInitialAccount: true);

            // Agregar segunda cuenta como no primaria
            var addSecondAccountCmd = new AddSupplierBankAccountCommand
            {
                BankName = "BDF",
                AccountNumber = "4433221100",
                AccountType = BankAccountType.Savings,
                Currency = Currency.NIO,
                AccountHolderName = "Proveedor Toggle S.A.",
                IsPrimary = false
            };

            var addRes = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addSecondAccountCmd);
            addRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var secondAccountDto = JsonSerializer.Deserialize<SupplierBankAccountDto>(await addRes.Content.ReadAsStringAsync(), SnakeCaseJsonOptions());

            // Actualizar cuenta 2 para que sea la primaria
            var updateCmd = new UpdateSupplierBankAccountCommand
            {
                IsPrimary = true
            };

            var patchRes = await SendAsync(
                HttpMethod.Patch,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{secondAccountDto!.Id}",
                updateCmd);

            patchRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var accountsInDb = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .ToListAsync();

            var firstAccount = accountsInDb.First(a => a.AccountNumber == "100200300");
            var secondAccount = accountsInDb.First(a => a.Id == secondAccountDto.Id);

            firstAccount.IsPrimary.Should().BeFalse();
            secondAccount.IsPrimary.Should().BeTrue();
        }

        [Test]
        public async Task UpdateSupplierBankAccount_WhenAccountNotFound_ReturnsBadRequest()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Cuenta No Existe S.A.", "0010909260105F", includeInitialAccount: true);
            var nonExistentAccountId = Guid.NewGuid();

            var updateCmd = new UpdateSupplierBankAccountCommand
            {
                BankName = "Banco Inexistente"
            };

            var patchRes = await SendAsync(
                HttpMethod.Patch,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{nonExistentAccountId}",
                updateCmd);

            patchRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task DeleteSupplierBankAccount_WhenNonPrimary_SoftDeletesAccount()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Delete S.A.", "0010909260106G", includeInitialAccount: true);

            // Agregar segunda cuenta no primaria
            var addCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Avanz",
                AccountNumber = "3322110099",
                AccountType = BankAccountType.Checking,
                Currency = Currency.USD,
                AccountHolderName = "Proveedor Delete S.A.",
                IsPrimary = false
            };

            var addRes = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addCmd);
            var secondAccount = JsonSerializer.Deserialize<SupplierBankAccountDto>(await addRes.Content.ReadAsStringAsync(), SnakeCaseJsonOptions());

            // Eliminar segunda cuenta
            var deleteRes = await SendAsync(
                HttpMethod.Delete,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{secondAccount!.Id}");

            deleteRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var deletedAccInDb = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(a => a.Id == secondAccount.Id);

            deletedAccInDb.DeletedAt.Should().NotBeNull();
            deletedAccInDb.IsPrimary.Should().BeFalse();

            var primaryAcc = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(a => a.AccountNumber == "100200300");

            primaryAcc.IsPrimary.Should().BeTrue();
            primaryAcc.DeletedAt.Should().BeNull();
        }

        [Test]
        public async Task DeleteSupplierBankAccount_WhenPrimary_ReassignsPrimaryToAnotherActiveAccount()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Reasignacion S.A.", "0010909260107H", includeInitialAccount: true);

            // Agregar segunda cuenta no primaria
            var addCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Ficohsa",
                AccountNumber = "6655443322",
                AccountType = BankAccountType.Savings,
                Currency = Currency.NIO,
                AccountHolderName = "Proveedor Reasignacion S.A.",
                IsPrimary = false
            };

            var addRes = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addCmd);
            var secondAccount = JsonSerializer.Deserialize<SupplierBankAccountDto>(await addRes.Content.ReadAsStringAsync(), SnakeCaseJsonOptions());

            // Cuenta 1 es la primaria actualmente
            var firstAccount = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(a => a.AccountNumber == "100200300");

            // Eliminar la cuenta primaria
            var deleteRes = await SendAsync(
                HttpMethod.Delete,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{firstAccount.Id}");

            deleteRes.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verificar que la primera cuenta fue soft-deleted y ya no es primaria
            var updatedFirstAccount = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(a => a.Id == firstAccount.Id);

            updatedFirstAccount.DeletedAt.Should().NotBeNull();
            updatedFirstAccount.IsPrimary.Should().BeFalse();

            // Verificar que la segunda cuenta activa fue promovida a primaria automáticamente
            var updatedSecondAccount = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .Where(s => s.Id == supplierId)
                .SelectMany(s => s.SupplierBankAccounts)
                .FirstAsync(a => a.Id == secondAccount!.Id);

            updatedSecondAccount.IsPrimary.Should().BeTrue();
            updatedSecondAccount.DeletedAt.Should().BeNull();
        }

        [Test]
        public async Task DeleteSupplierBankAccount_WhenAccountNotFound_ReturnsBadRequest()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Delete Inexistente S.A.", "0010909260108I", includeInitialAccount: true);
            var nonExistentAccountId = Guid.NewGuid();

            var response = await SendAsync(
                HttpMethod.Delete,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{nonExistentAccountId}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSupplierBankAccounts_ReturnsOnlyActiveAccounts()
        {
            var (companyId, supplierId) = await CreateSupplierAsync("Proveedor Lista Cuentas S.A.", "0010909260109J", includeInitialAccount: true);

            // Agregar segunda cuenta
            var addCmd = new AddSupplierBankAccountCommand
            {
                BankName = "Banco Activo",
                AccountNumber = "1122334455",
                AccountType = BankAccountType.Checking,
                Currency = Currency.USD,
                AccountHolderName = "Proveedor Lista Cuentas S.A.",
                IsPrimary = false
            };

            var addRes = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts",
                addCmd);
            var secondAccount = JsonSerializer.Deserialize<SupplierBankAccountDto>(await addRes.Content.ReadAsStringAsync(), SnakeCaseJsonOptions());

            // Eliminar segunda cuenta
            await SendAsync(
                HttpMethod.Delete,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts/{secondAccount!.Id}");

            // Consultar cuentas bancarias del proveedor
            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{companyId}/modules/{ModuleCode}/suppliers/{supplierId}/bank-accounts");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var accounts = JsonSerializer.Deserialize<List<SupplierBankAccountDto>>(json, SnakeCaseJsonOptions());

            accounts.Should().NotBeNull();
            accounts!.Should().HaveCount(1);
            accounts!.First().AccountNumber.Should().Be("100200300"); // Solo la primera sigue activa
        }

        [Test]
        public async Task GetSupplierBankAccounts_WhenSupplierNotFound_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");
            var nonExistentSupplierId = Guid.NewGuid();

            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{nonExistentSupplierId}/bank-accounts");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
