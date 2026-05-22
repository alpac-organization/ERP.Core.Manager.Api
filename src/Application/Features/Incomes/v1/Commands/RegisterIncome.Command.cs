using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands
{
    public class RegisterIncomeCommand: BaseRequest, IRequest<bool>
    {
        public Guid BranchId { get; set; }
        public Guid PayrollId { get; set; }
        public Guid TypeIncomeId  { get; set; }
        
        public BonusPayload? BonusPayload { get; set; }
        public CommissionsPayload? CommissionsPayload { get; set; }
        public List<OvertimeIncomeData> OvertimeIncomeData { get; set; } = [];
    }

    public class OvertimeIncomeData
    {
        public string? IdentificationNumber { get; set; }
        public decimal AmountHours { get; set; }
    }

    public class CommissionsPayload
    {
        public Currency Currency { get; set; }
        public decimal CommissionAmount { get; set; }
        public string? IdentificationNumber { get; set; }
    }

    public class BonusPayload
    {
        public Currency Currency { get; set; }
        public decimal BonusAmount { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}