using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries
{
    public class GetPermitApplicationByCollaboratorCodeQuery: BaseRequest, IRequest<PermitApplicationDto>
    {
        public string? CollaboratorCode { get; set; }
    }
}