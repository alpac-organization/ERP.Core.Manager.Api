using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class RegisterCollaboratorCommand: BaseRequest, IRequest<bool>
    {
        /// <summary>
        /// Indicar si el colaborador trabaja de lunes a sabados
        /// </summary>
        public bool DoesWorkSaturday { get; set; } = false;

        public string? FirstName { get; set; }
        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }

        public string? FirstLastname { get; set; }
        public string? SecondLastname { get; set; }        
        public string? IdentificationNumber { get; set; }

        public IdentificationType IdentificationType { get; set; }

        required public SalaryInformationCommand SalaryInformation { get; set; }
        required public WorkingInformationCommand WorkingInformation { get; set; }
        required public PersonalInformationCommand PersonalInformation { get; set; }

        public List<TravelExpenses> TravelExpenses { get; set; } = [];
    }

    public class WorkingInformationCommand
    {
        public string? Daem { get; set; }
        public string? WorkEmail { get; set; }
        public string? InssNumber { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public string? BankAccountNumber { get; set; }

        //Catalogs
        public Guid AreaId { get; set; }
        public Guid BranchId { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid JobPositionId { get; set; }

        public DateOnly EntryDate { get; set; }
    }

    public class PersonalInformationCommand
    {
        public string? Address { get; set; }
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }

        public GenderType Gender { get; set; }
        public DateTime Birthdate { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
    }

    public class SalaryInformationCommand
    {
        public decimal Salary { get; set; }
        public int SubCatalogBankId { get; set; }

        public Currency Currency { get; set; }
        public SalaryType SalaryType { get; set; }
    }

    public class TravelExpenses
    {
        public Guid TypeIncomeId { get; set; }
        public decimal IncomeAmount { get; set; }
    }
}