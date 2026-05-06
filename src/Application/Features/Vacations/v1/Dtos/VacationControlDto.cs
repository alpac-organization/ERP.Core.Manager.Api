namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos
{
    public class VacationAccruals
    {
        public Guid VacationId { get; set; }
        public decimal VacationBalance { get; set; }
        public decimal EnjoyedVacations { get; set; }

        public CollaboratorInformation? CollaboratorInformation { get; set; }
    }

    public class CollaboratorInformation
    {
        public string? Code { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorFullname { get; set; }
        public string? IdentificationNumber { get; set; }

        public DateTime EntryDate { get; set; }
    }
}