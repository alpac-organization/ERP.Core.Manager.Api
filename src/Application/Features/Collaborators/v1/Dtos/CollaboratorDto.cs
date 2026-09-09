using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class CollaboratorDto
    {
        public Guid CollaboratorId { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? FirstLastname { get; set; }
        public string? BranchName { get; set; }
        public string? WorkArea { get; set; }
        public decimal Vacations { get; set; }
        public string? WorkPosition { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? IdentificationNumber { get; set; }
        public CollaboratorStatus Status { get; set; }
    }

    public record PagedResponse<T>(
        List<T> Data, 
        int PageSize,
        int PageNumber, 
        int TotalRecords,

        int TotalActive        = 0,
        int TotalOnVacation    = 0,
        int TotalOnSubsidy     = 0,
        int TotalCollaborators = 0 
    );
}   