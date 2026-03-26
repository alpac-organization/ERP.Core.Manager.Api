using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class RegisterCollaboratorCommand: IRequest<bool>
    {
        //Para mapear que el usuario tenga permiso para registrar datos de colaborador
        public Guid UserId { get; set; }

        //Codigo del modulo para    
        public string? ModuleCode { get; set; }
        public string? Code { get; set; }

        public string? FirstName { get; set; }
        public string? FirstLastname { get; set; }
        public string? IdentificationNumber { get; set; }
        public Guid CompanyId { get; set; }

        public string? SecondName { get; set; }
        public string? ThirdName { get; set; }
        public string? SecondLastname { get; set; }
        public string? RegisteredBy { get; set; }

        public GenderType Gender { get; set; }
        public CollaboratorStatus Status { get; set; }
        public IdentificationType IdentificationType { get; set; }


        public WorkingInformation? WorkingInformation { get; set; }
        public PersonalInformation? PersonalInformation { get; set; }
        public SalaryInformation? SalaryInformation { get; set; }
    }

    public class WorkingInformation
    {
        public string? BankAccountNumber { get; set; }
        public string? WorkPhonNumber { get; set; }
        public string? WorkEmail { get; set; }
        public string? InssNumber { get; set; }

        public int WorkAreaId { get; set; }
        public int WorkPositionId { get; set; }
        public string? BranchId { get; set; }


        public DateTime EntryDate { get; set; }
    }

    public class PersonalInformation
    {
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Departament { get; set; }
        public DateTime Birthdate { get; set; }
    }

    public class SalaryInformation
    {
        public Currency Currency { get; set; }
        public SalaryType SalaryType { get; set; }
        public decimal Salary { get; set; }
    }
}