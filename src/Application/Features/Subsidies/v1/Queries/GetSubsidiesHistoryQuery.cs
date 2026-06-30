using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Queries
{
    public class GetSubsidiesHistoryQuery : BaseRequest, IRequest<PagedResponse<SubsidyHistoryDto>>
    {
        public Guid PayrollId { get; set; }

        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}