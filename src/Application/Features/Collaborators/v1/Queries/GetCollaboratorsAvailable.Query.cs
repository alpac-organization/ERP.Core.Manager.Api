using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

using BaseRequest = ERP.Core.Manager.Api.Domain.Entities.Bases.BaseRequest;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorsAvailableQuery : BaseRequest, IRequest<PagedResponse<CollaboratorDto>>
    {

        public Guid? AreaId { get; set; }
        public Guid? BranchId  { get; set; }
        public string? IdentificationNumber { get; set; }
        public CollaboratorStatus? Status { get; set; }

        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
