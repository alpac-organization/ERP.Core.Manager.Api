using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries
{
    public class GetDeductionsActiveQuery: BaseRequest, IRequest<PagedResponseDeduction<DeductionDto>>
    {
        public string? IdentificationNumber { get; set; }
        public DeductionType? DeductionType { get; set; }
        public DeductionStatus? DeductionStatus { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}