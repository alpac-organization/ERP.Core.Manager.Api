using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Queries
{
    public class GetSubsidiesHistoryQuery : BaseRequest, IRequest<PagedResponse<SubsidyHistoryDto>>
    {
        public string? IdentificationNumber { get; set; }
        public Guid? AreaId { get; set; }
        public int PageSize { get; set; }
        public Guid BranchId { get; set; }
        public int PageNumber { get; set; }
    }
}