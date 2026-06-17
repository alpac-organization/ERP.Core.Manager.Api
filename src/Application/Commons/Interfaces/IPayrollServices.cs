using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
    public interface IPayrollServices
    {   
        //Obtener lista de colaboradores por tipos de salarios.
        Task<List<Collaborator>> ObtainsCollaboratorByType(SalaryType salaryType, Guid companyId, Guid branchId);

        //Asignar dias de viaticos para este colaborador a pagar
        Task<int> AssignTravelDays(Collaborator collaborator, DateOnly payrollStart, DateOnly payrollEnd);

        //Asignar control de vacaciones
        Task AssignVacationControl(Collaborator collaborator);

        //Asignas viaticos del colaborador
        Task AssignTravelAllowance(Collaborator collaborator, List<TravelExpenses> travelExpenses);

        //Asignar Salario al colaborador
        Task<bool> AssignSalary(Collaborator collaborator, SalaryInformation salaryInformation);

        Task RegisterCollaboratorToPayroll(Guid payrollId, Collaborator collaborator, CancellationToken cancellationToken, bool isFirstTimes = false);

        Task RegisterCollaboratorToVigemsaProfessional(Guid payrollId, Collaborator collaborator);

        Task RegisterCollaboratorToAvasaTransport();
    }
}