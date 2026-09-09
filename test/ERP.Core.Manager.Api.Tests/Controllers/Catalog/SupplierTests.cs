using System.Net;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using NUnit.Framework;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Tests.Common;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Tests.Controllers.Catalog
{
    [TestFixture]
    public class SupplierTests : IntegrationTestBase
    {
        private const string ModuleCode = "PURCHASING";

        [Test]
        public async Task RegisterSupplier_HappyPath_PersistsSupplierAndDetails()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Comercial del Valle S.A.",
                CommercialName = "Comercial Valle",
                IdentificationNumber = "0010909260001A",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Km 5 Carretera Norte",
                    EmailSupport = "soporte@valle.com",
                    ContactName = "Juan Perez",
                    ContactEmail = "juan@valle.com",
                    ContactPhoneNumber = "+50588889999",
                    HasCredit = true,
                    CreditDays = 30,
                    CreditLimit = 50000m,
                    CreditCurrency = Currency.USD,
                    PreferredPaymentMethod = PaymentMethodType.ACH,
                    IsExclusive = true,
                    ExclusiveBrandsOrParts = "Marca Exclusiva",
                    ApplyIrRetention = true,
                    ApplyMunicipalRetention = true,
                    IsTaxExempt = false
                },
                BankAccounts = new List<SupplierBankAccountCommand>
                {
                    new()
                    {
                        BankName = "BAC Credomatic",
                        AccountNumber = "9988776655",
                        AccountType = BankAccountType.Checking,
                        Currency = Currency.USD,
                        AccountHolderName = "Comercial del Valle S.A.",
                        IsPrimary = true
                    }
                }
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegisterSupplierDto>(json, SnakeCaseJsonOptions());

            result.Should().NotBeNull();
            result!.SupplierId.Should().NotBeEmpty();

            var supplier = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierDetails)
                .Include(s => s.SupplierBankAccounts)
                .FirstOrDefaultAsync(s => s.Id == result.SupplierId);

            supplier.Should().NotBeNull();
            supplier!.CommercialName.Should().Be("Comercial Valle");
            supplier.SupplierDetails.Should().NotBeNull();
            supplier.SupplierDetails.HasCredit.Should().BeTrue();
            supplier.SupplierDetails.CreditDays.Should().Be(30);
            supplier.SupplierDetails.CreditLimit.Should().Be(50000m);
            supplier.SupplierBankAccounts.Should().HaveCount(1);
            supplier.SupplierBankAccounts.First().IsPrimary.Should().BeTrue();
        }

        [Test]
        public async Task RegisterSupplier_WhenCreditDaysLessThanOne_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Proveedor Sin Dias S.A.",
                CommercialName = "Sin Dias",
                IdentificationNumber = "0010909260002B",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    HasCredit = true,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task RegisterSupplier_WhenMoreThanOnePrimaryBankAccount_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Dos Cuentas Primarias S.A.",
                CommercialName = "Dos Primarias",
                IdentificationNumber = "0010909260003C",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                },
                BankAccounts = new List<SupplierBankAccountCommand>
                {
                    new() { BankName = "Banco 1", AccountNumber = "111", AccountType = BankAccountType.Savings, IsPrimary = true, AccountHolderName = "Titular 1" },
                    new() { BankName = "Banco 2", AccountNumber = "222", AccountType = BankAccountType.Checking, IsPrimary = true, AccountHolderName = "Titular 2" }
                }
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSuppliers_FilterByCommercialName_ReturnsOnlyMatching()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var cmd1 = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Empresa Alfa S.A.",
                CommercialName = "Alfa Distribuciones",
                IdentificationNumber = "0010909260010A",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Managua",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };
            var cmd2 = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Empresa Beta S.A.",
                CommercialName = "Beta Logistica",
                IdentificationNumber = "0010909260011B",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Leon",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };

            await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", cmd1);
            await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", cmd2);

            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers?commercial_name=Alfa");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<SupplierDto>>(json, SnakeCaseJsonOptions());

            paged.Should().NotBeNull();
            paged!.Data.Should().Contain(s => s.CommercialName == "Alfa Distribuciones");
            paged.Data.Should().NotContain(s => s.CommercialName == "Beta Logistica");
        }

        [Test]
        public async Task UpdateSupplierInformation_PartialUpdate_UpdatesOnlySpecifiedFields()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var registerCmd = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Original S.A.",
                CommercialName = "Original",
                IdentificationNumber = "0010909260020X",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Original Address",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };

            var createRes = await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", registerCmd);
            var createJson = await createRes.Content.ReadAsStringAsync();
            var created = JsonSerializer.Deserialize<RegisterSupplierDto>(createJson, SnakeCaseJsonOptions());

            var updateCmd = new UpdateSupplierInformationCommand
            {
                CommercialName = "Actualizado Comercial",
                SupplierDetails = new SupplierDetailsInformation
                {
                    Address = "Direccion Nueva",
                    HasCredit = true,
                    CreditDays = 15,
                    CreditLimit = 25000m
                }
            };

            var patchRes = await SendAsync(
                HttpMethod.Patch,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{created!.SupplierId}",
                updateCmd);

            patchRes.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedSupplier = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierDetails)
                .FirstAsync(s => s.Id == created.SupplierId);

            updatedSupplier.SuppliersLegalName.Should().Be("Original S.A.");
            updatedSupplier.CommercialName.Should().Be("Actualizado Comercial");
            updatedSupplier.SupplierDetails.Address.Should().Be("Direccion Nueva");
            updatedSupplier.SupplierDetails.HasCredit.Should().BeTrue();
            updatedSupplier.SupplierDetails.CreditDays.Should().Be(15);
            updatedSupplier.SupplierDetails.CreditLimit.Should().Be(25000m);
        }

        [Test]
        public async Task GetSupplierDetails_ReturnsSupplierWithDetailsAndBankAccounts()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var registerCmd = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Detalles Full S.A.",
                CommercialName = "Full Details",
                IdentificationNumber = "0010909260030Z",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Zona Franca",
                    HasCredit = true,
                    CreditDays = 45,
                    CreditLimit = 80000m,
                    PreferredPaymentMethod = PaymentMethodType.InternationalWire
                },
                BankAccounts = new List<SupplierBankAccountCommand>
                {
                    new()
                    {
                        BankName = "LAFISE",
                        AccountNumber = "123456789",
                        AccountType = BankAccountType.Savings,
                        Currency = Currency.NIO,
                        AccountHolderName = "Detalles Full S.A.",
                        IsPrimary = true
                    }
                }
            };

            var createRes = await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", registerCmd);
            var createJson = await createRes.Content.ReadAsStringAsync();
            var created = JsonSerializer.Deserialize<RegisterSupplierDto>(createJson, SnakeCaseJsonOptions());

            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{created!.SupplierId}/details");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var details = JsonSerializer.Deserialize<SupplierInformationDto>(json, SnakeCaseJsonOptions());

            details.Should().NotBeNull();
            details!.CommercialName.Should().Be("Full Details");
            details.SupplierDetails.Should().NotBeNull();
            details.SupplierDetails.CreditDays.Should().Be(45);
            details.SupplierDetails.PreferredPaymentMethod.Should().Be(PaymentMethodType.InternationalWire);
            details.BankAccounts.Should().HaveCount(1);
            details.BankAccounts.First().AccountNumber.Should().Be("123456789");
            details.BankAccounts.First().IsPrimary.Should().BeTrue();
        }

        [Test]
        public async Task RegisterSupplier_WithoutBankAccounts_Success()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Proveedor Sin Bancos S.A.",
                CommercialName = "Sin Bancos",
                IdentificationNumber = "0010909260040A",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Managua, Nicaragua",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                },
                BankAccounts = new List<SupplierBankAccountCommand>()
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegisterSupplierDto>(json, SnakeCaseJsonOptions());

            result.Should().NotBeNull();
            result!.SupplierId.Should().NotBeEmpty();

            var supplier = await UnitOfWork.Suppliers.Entities
                .AsNoTracking()
                .Include(s => s.SupplierBankAccounts)
                .FirstOrDefaultAsync(s => s.Id == result.SupplierId);

            supplier.Should().NotBeNull();
            supplier!.SupplierBankAccounts.Should().BeEmpty();
        }

        [Test]
        public async Task RegisterSupplier_WhenIdentificationNumberMissing_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var command = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Proveedor Sin Cedula S.A.",
                CommercialName = "Sin Cedula",
                IdentificationNumber = "", // Empty
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };

            var response = await SendAsync(
                HttpMethod.Post,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers",
                command);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSuppliers_FilterByConstitutionType_ReturnsOnlyMatching()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");

            var cmdJuridica = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Persona Juridica S.A.",
                CommercialName = "PJ S.A.",
                IdentificationNumber = "0010909260050A",
                ConstitutionType = ConstitutionType.Legal,
                IdentificationType = IdentificationType.Ruc,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Managua",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };
            var cmdNatural = new RegisterSupplierCommand
            {
                SuppliersLegalName = "Juan Perez Natural",
                CommercialName = "Juan Natural",
                IdentificationNumber = "0010909260051B",
                ConstitutionType = ConstitutionType.Natural,
                IdentificationType = IdentificationType.Cedula,
                SupplierDetails = new SupplierDetails
                {
                    Address = "Granada",
                    HasCredit = false,
                    CreditDays = 0,
                    PreferredPaymentMethod = PaymentMethodType.ACH
                }
            };

            await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", cmdJuridica);
            await SendAsync(HttpMethod.Post, $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers", cmdNatural);

            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers?constitution_type={ConstitutionType.Natural}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            var paged = JsonSerializer.Deserialize<PagedResponse<SupplierDto>>(json, SnakeCaseJsonOptions());

            paged.Should().NotBeNull();
            paged!.Data.Should().Contain(s => s.CommercialName == "Juan Natural");
            paged.Data.Should().NotContain(s => s.CommercialName == "PJ S.A.");
        }

        [Test]
        public async Task UpdateSupplierInformation_WhenSupplierNotFound_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");
            var nonExistentId = Guid.NewGuid();

            var updateCmd = new UpdateSupplierInformationCommand
            {
                CommercialName = "No Existe"
            };

            var patchRes = await SendAsync(
                HttpMethod.Patch,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{nonExistentId}",
                updateCmd);

            patchRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task GetSupplierDetails_WhenSupplierNotFound_ReturnsBadRequest()
        {
            var company = await UnitOfWork.Companies.Entities.FirstAsync(c => c.Alias == "ALPAC");
            var nonExistentId = Guid.NewGuid();

            var response = await SendAsync(
                HttpMethod.Get,
                $"/api/v1/companies/{company.Id}/modules/{ModuleCode}/suppliers/{nonExistentId}/details");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
