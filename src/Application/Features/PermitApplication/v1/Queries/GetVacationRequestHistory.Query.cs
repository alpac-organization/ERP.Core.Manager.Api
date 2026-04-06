using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetPermitApplicationHistoryQuery: BaseRequest, IRequest<List<PermitApplicationDto>>
    {
        public string? IdentificationNumber { get; set; }
        public PermitApplicationStatus? Status { get; set; }

        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}