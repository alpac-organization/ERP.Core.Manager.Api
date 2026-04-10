using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetVacationBalanceQuery : BaseRequest, IRequest<VacationDto>
    {
        public string? IdentificationNumber { get; set; }
    }
}