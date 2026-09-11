using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class CollaboratorDetailsDto
    {        
        public Guid CollaboratorId { get; set; }
        public string? FullName{ get; set; }
        public string? CollaboratorCode { get; set; }
        public string? WorkPosition { get; set; }

        public WorkingInformationDto  WorkingInformation { get; set; } = new();
        public PersonalInformationDto PersonalInformation { get; set; } = new();
        public SalaryInformationDto SalaryInformation { get; set; } = new(); 
        public VacationInformationDto VacationInformation { get; set; } = new();  
    }
    
    public class PersonalInformationDto
    {
        public Guid PersonalInformationId { get; set; }
        public string? Address { get; set; }
        public string? PersonalEmail { get; set; }
        public string? PersonalPhoneNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public GenderType Gender { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public class WorkingInformationDto
    {
        public Guid WorkingInformationId { get; set; }

        public string? Daem { get; set; }
        public string? WorkEmail { get; set; }
        public string? InssNumber { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public string? BankAccountNumber { get; set; }

        public string? WorkArea { get; set; }
        public string? JobPosition { get; set; }
        public string? CostCenter { get; set; }

        public DateOnly EntryDate { get; set; }
        public DateOnly? DepartureDate { get; set; }    
    }

    public class SalaryInformationDto
    {
        public decimal Salary { get; set; }
        public string? Currency { get; set;}
        public string? SalaryType { get; set; }
    }

    public class VacationInformationDto
    {
        public decimal AvailableVacations { get; set; }
    }

}