namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos
{
    public class GetCollaboratorDto
    {
        public string? FirstName { get; set; }
        public string? FirstLastname { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? CollaboratorCode { get; set; }
    }
}