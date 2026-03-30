using MediatR;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorsAvailableQuery : IRequest<List<GetCollaboratorDto>>
    {
        public string? Status { get; set; }
        public string? IdentificationNumber { get; set; }
        public int BranchSubCatalogId  { get; set; }
        public int AreaSubCatalogId { get; set; }
    }
}