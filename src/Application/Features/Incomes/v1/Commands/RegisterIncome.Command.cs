using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands
{
    public class RegisterIncomeCommand: BaseRequest, IRequest<bool>
    {
        public decimal IncomeAmount { get; set; }
        public string? IdentificationNumber { get; set; }
        public Guid TypeIncomeId { get; set; }
        public int AmountHours { get; set; }
        public string? Description { get; set; }
    }
}