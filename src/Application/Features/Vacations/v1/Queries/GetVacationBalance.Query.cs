using MediatR;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries
{
    public class GetVacationBalanceQuery : IRequest<VacationDto>
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string? ModuleCode { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}