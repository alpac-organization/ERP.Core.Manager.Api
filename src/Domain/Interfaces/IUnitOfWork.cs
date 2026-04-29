using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Catalogs;
namespace ERP.Core.Manager.Api.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsersRepository Users { get; }
        IModulesRepository Modules { get; }
        ICompaniesRepository Companies { get; }
        IUserProfilesRepository Profiles { get; }
        ISessionsRepository Sessions { get; }
        IRolesRepository Roles { get; }
        IUserModulesRoleRepository UserModules { get; }
        ICatalogsRepository CatalogsRepository { get; }
        ISubCatalogsRepository SubCatalogs { get; }
        ICollaboratorsRepository Collaborators { get; }
        IWorkingInformationRepository WorkingInformations { get; }
        IPersonalInformationRepository PersonalInformations { get; }
        ISalariesRepository Salaries { get; }
        IVacationsRepository Vacations { get; }
        IPermitApplicationsRepository PermitApplications { get; }
        IDeductionsRepository Deductions { get; }
        IPayrollsRepository Payrolls { get; }
        IOrdinaryPayrollsRepository OrdinaryPayrolls { get; }
        IWorkPositionsHistoryRepository WorkPositionHistories { get; }
        IValidityDeductionsRepository ValidityDeductions { get; }
        IBranchesRepository Branches { get; }
        IIncomesRepository Incomes { get; }
        ITypesIncomeRepository TypesIncome { get; }
        IIncomeTaxAccrualRepository IncomeTaxAccrual { get;}
        IAssignedTravelExpensesHistoryRepository AssignedTravelExpensesHistories  { get; }


        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}