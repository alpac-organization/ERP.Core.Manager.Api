using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Shopping;
using ERP.Core.Database.Domain.Entities.Accounting;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Tests.Common.Utils;
using System.Text.Json;

namespace ERP.Core.Manager.Api.Tests.Common
{
    public class IntegrationTestUtilsBase : IntegrationTestBase
    {

        //All companies here for ERP-System!, las demas clases lo heredan la Data structure.
        protected static readonly string[] AllCompanies = ["ALPAC", "AMINSA", "AVASA", "VIGEMSA", "TMN"];

        // crear usuario Dinámico por compañía
        public async Task<Guid> CreateUser(string fullname, Guid? areaId = null, string companyAlias = "ALPAC")
        {
            var company = await _unitOfWork.Companies.Entities
                .FirstAsync(c => c.Alias == companyAlias);

            Guid workAreaId;
            if (areaId.HasValue) workAreaId = areaId.Value;
            else
            {
                var area = await _unitOfWork.WorkAreas.Entities
                    .Where(w => w.IsActive && w.CompanyId == company.Id && w.WorkAreaCode == 10)
                    .FirstAsync();
                workAreaId = area.Id;
            }

            var newUserId = Guid.NewGuid();
            var suffix = newUserId.ToString("N")[..8];

            await _unitOfWork.Users.CreateNewUser(new()
            {
                Id = newUserId,
                UserStatus = UserStatus.Active,
                UserType = UserType.StandardUser,
                UserName = $"user.{suffix}",
                PasswordHash = "$hashpassword",
                Fullname = fullname,
                Email = $"testing.{suffix}@domain.com",
                IdentificationNumber = $"001{suffix}A",
            });

            var branch = await _unitOfWork.Branches.Entities
                .Where(b => b.IsActive && b.CompanyId == company.Id)
                .FirstAsync();

            var profileId = Guid.NewGuid();

            await _unitOfWork.Profiles.CreateNewUserProfile(new()
            {
                Id = profileId,
                UserId = newUserId,
                BranchId = branch.Id,
                IsActive = true,
                CompanyId = company.Id
            });

            await _unitOfWork.SaveChangesAsync(default);
            return newUserId;
        }

        //  otorgar acceso al modulo 
        public async Task GrantModuleAccessAsync(Guid userId, Guid companyId, RoleType roleType, string moduleCode)
        {
            var profile = await _unitOfWork.Profiles.Entities
                .Where(pr => pr.UserId == userId && pr.CompanyId == companyId)
                .FirstOrDefaultAsync(default);

            var role = await _unitOfWork.Roles.Entities
                .Where(r => r.RoleType == roleType)
                .FirstOrDefaultAsync(default);

            var module = await _unitOfWork.Modules.Entities
                .Where(m => m.Code == moduleCode)
                .FirstOrDefaultAsync(default);

            await _unitOfWork.UserModules.AssignRolesModule(new UserModuleRoles
            {
                Id = Guid.NewGuid(),
                UserProfileId = profile!.Id,
                RoleId = role!.Id,
                ModuleId = module!.Id,
                ModuleCode = moduleCode,
                IsActive = true
            });

            await _unitOfWork.SaveChangesAsync(default);
        }

        protected async Task<(Guid CompanyId, Guid UserId, string Token)> ArrangeUserWithRole(
            RoleType roleType,
            string companyAlias = "ALPAC",
            string moduleCode = "NOMINA")
        {
            var company = await _unitOfWork.Companies.Entities
                .Where(c => c.IsActive)
                .FirstAsync(c => c.Alias == companyAlias);

            var userId = await CreateUser($"Test User {roleType}", companyAlias: companyAlias);

            await GrantModuleAccessAsync(userId, company.Id, roleType, moduleCode);

            var token = AuthManager.GenerateJwtToken(EnvironmentManager.JwtKey, userId);
            return (company.Id, userId, token);
        }

        protected static async Task<string?> ReadErrorType(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return null;

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!TryGetPropertyIgnoreCase(root, "error", out var error))
                return null;

            if (TryGetPropertyIgnoreCase(error, "typeError", out var type) || TryGetPropertyIgnoreCase(error, "type_error", out type))
            {
                return type.GetString();
            }

            return null;
        }

        private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            if (element.TryGetProperty(name, out value)) return true;
            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        #region Payroll / Collaborators

        public async Task<Collaborator> GetOrCreateCollaboratorAsync(Guid? userId = null)
        {
            var collaborator = await _unitOfWork.Collaborators.Entities
                .FirstOrDefaultAsync();

            if (collaborator == null)
            {
                var targetUserId = userId.HasValue && userId.Value != Guid.Empty
                    ? userId.Value
                    : await _unitOfWork.Users.Entities.Select(u => u.Id).FirstOrDefaultAsync();

                collaborator = new Collaborator
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test",
                    FirstLastname = "Collaborator",
                    IdentificationNumber = "0012345678",
                    CollaboratorCode = "TC-001",
                    IdentificationType = IdentificationType.Cedula,
                    CompanyId = TestCompanies.Alpac,
                    Status = CollaboratorStatus.Active,
                    IsFirstTimeRegister = false,
                    RegisteredBy = "Test System",
                    PictureUrl = null,
                    DoesWorkSaturdays = false,
                    HasBeenFired = false
                };
                await _unitOfWork.Collaborators.RegisterCollaborator(collaborator);
                await _unitOfWork.SaveChangesAsync(default);
            }

            return collaborator;
        }

        public async Task<PersonalInformation> CreatePersonalInformation(Guid collaboratorId)
        {
            var personalInfo = new PersonalInformation
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                Address = "Test Address",
                PersonalEmail = "test.collaborator@domain.com",
                PersonalPhoneNumber = "88888888",
                Birthdate = DateTime.UtcNow.AddYears(-30),
                Gender = GenderType.Man,
                MaritalStatus = MaritalStatus.Single
            };

            await _unitOfWork.PersonalInformations.RegisterPersonalInformation(personalInfo);
            await _unitOfWork.SaveChangesAsync(default);

            return personalInfo;
        }

        public async Task<WorkingInformation> CreateWorkingInformation(Guid collaboratorId, Guid areaId, Guid branchId, Guid jobPositionId, Guid? costCenterId = null)
        {
            var workingInfo = new WorkingInformation
            {
                Id = Guid.NewGuid(),
                CollaboratorId = collaboratorId,
                AreaId = areaId,
                BranchId = branchId,
                JobPositionId = jobPositionId,
                CostCenterId = costCenterId,
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Daem = "DAEM-001",
                WorkEmail = "test@work.com",
                InssNumber = "INSS-001",
                WorkPhoneNumber = "88888888",
                BankAccountNumber = "1234567890"
            };

            await _unitOfWork.WorkingInformations.RegisterWorkingInformation(workingInfo);
            await _unitOfWork.SaveChangesAsync(default);

            return workingInfo;
        }

        #endregion
    }
}