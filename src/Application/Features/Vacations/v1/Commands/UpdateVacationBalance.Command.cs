using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands
{
    public class UpdateVacationBalanceCommand: BaseRequest, IRequest<bool>
    {
        public Guid VacationId { get; set; }
        public string? IdentificationNumber { get; set; }
        public decimal VacationBalance { get; set; } 
        public decimal EnjoyedVacation { get; set; }
    }
}