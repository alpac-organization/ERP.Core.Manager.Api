using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries
{
    public class GetDeductionDetailsQuery: BaseRequest, IRequest<DeductionDetailsDto>
    {
        public Guid DeductionId { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}