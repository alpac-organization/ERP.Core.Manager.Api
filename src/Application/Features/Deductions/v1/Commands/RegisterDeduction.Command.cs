using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands
{
    public class RegisterDeductionCommand: BaseRequest, IRequest<bool>
    {
        public Guid CollaboratorId { get; set; }
        public string? Description { get; set; }
        public DeductionType DeductionType { get; set; }

        //Payloads de deducciones
        // public LoansPayload? LoansPayload { get; set; }
        public AdvanceSalaryPayload? AdvanceSalaryPayload { get; set; }
        public LateArrivalsPayload? LateArrivalsPayload { get; set; }
    }

    public class LateArrivalsPayload
    {
        public decimal TotalMinutes { get; set; }
    }

    public class AdvanceSalaryPayload
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
    }

    public class LoansPayload
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int NumberFortnights { get; set; }
    }
}