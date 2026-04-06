using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetVacationRequestQuery : BaseRequest, IRequest<List<PermitApplicationRequestDto>>
    {
        public string? IdentificationNumber { get; set; }
        public PermitApplicationStatus? Status { get; set; }

        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }
}
