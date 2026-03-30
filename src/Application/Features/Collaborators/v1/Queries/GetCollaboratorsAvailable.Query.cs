using MediatR;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorsAvailableQuery : IRequest<PagedResponse<GetCollaboratorDto>>
    {

        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public CollaboratorStatus? Status { get; set; }


        public string? ModuleCode { get; set; }
        public string? IdentificationNumber { get; set; }
        public int BranchSubCatalogId  { get; set; }
        public int AreaSubCatalogId { get; set; }


        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
