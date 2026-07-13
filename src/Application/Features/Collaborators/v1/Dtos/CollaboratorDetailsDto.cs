using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class CollaboratorDetailsDto
    {
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? FullName { get; set; }
        public string? WorkPosition { get; set; }
        public string?  Status { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public List<CostCenterDto> CostCenters { get; set; } = [];
        public PersonalInformationDto PersonalInformation { get; set; } = new();
        public WorkingInformationDto WorkingInformation { get; set; } = new();
        public SalaryInformationDto SalaryInformation { get; set; } = new(); 
        public VacationInformationDto VacationInformation { get; set; } = new();  
    }

    public class PersonalInformationDto
    {
        public GenderType Gender { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Address { get; set; }
        public string? PersonalEmail { get; set;}
        public string? PersonalPhoneNumber { get; set; }
        public string? Departament { get; set; }
        public MaritalStatus MaritalStatus { get; set; }
        public DateTime Birthdate { get; set; }
    }

    public class WorkingInformationDto
    {
        public string? InssNumber { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public string? WorkEmail { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? WorkArea { get; set; }
        public string? WorkPosition { get; set; }
        public string? BranchName { get; set; } 
        public DateOnly EntryDate { get; set; }
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