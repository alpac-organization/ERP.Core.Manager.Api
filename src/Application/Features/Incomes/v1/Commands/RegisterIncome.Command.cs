using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands
{
    public class RegisterIncomeCommand: BaseRequest, IRequest<bool>
    {
        public Guid PayrollId { get; set; }
        public Guid TypeIncomeId  { get; set; }

        public string? Description { get; set; }
        public string? IdentificationNumber { get; set; }

        //Ingreso de horas extras
        public OvertimeIncomePayload? OvertimeIncomePayload { get; set; }
    }

    public class OvertimeIncomePayload
    {
        public decimal AmountHours { get; set; }
    }
}