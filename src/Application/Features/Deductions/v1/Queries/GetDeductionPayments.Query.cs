using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries
{
    public class GetDeductionPaymentsQuery: BaseRequest, IRequest<PagedResponseDeduction<DeductionPaymentsDto>>
    {
        public Guid DeductionId { get; set; }

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}