namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class GetCollaboratorDto
    {
        public Guid CollaboratorId { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? FirstLastname { get; set; }
        public string? Status { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? CollaboratorCode { get; set; }
        public string? WorkArea { get; set; }
        public string? WorkPosition { get; set; }
    }

   public record PagedResponse<T>(
        List<T> Data, 
        int TotalRecords, 
        int PageNumber, 
        int PageSize,

        int TotalActive = 0,
        int TotalOnVacation = 0,
        int TotalOnSubsidy = 0,
        int TotalCollaborators = 0 
    );
}   