namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorsAvailableQuery
    {
        public string? IdentificationNumber { get; set; }
        public string? BranchId  { get; set; }
        public string? Status { get; set; }
    }
}