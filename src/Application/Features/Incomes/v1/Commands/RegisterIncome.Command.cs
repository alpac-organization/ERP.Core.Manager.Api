using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands
{
    public class RegisterIncomeCommand: BaseRequest, IRequest<bool>
    {
        public Guid PayrollId { get; set; }
        public Guid TypeIncomeId  { get; set; }

        //Ingreso de horas extras
        // public OvertimeIncomePayload? OvertimeIncomePayload { get; set; }
        public CommissionsPayload? CommissionsPayload { get; set; }
    }

    public class OvertimeIncomePayload
    {
        public decimal AmountHours { get; set; }
    }

    public class CommissionsPayload
    {
        public Currency Currency { get; set; }
        public bool ItFree { get; set; } = false;
        public int?  CommissionPercentage { get; set; }
        public decimal CommissionAmount { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}