using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos
{
    public class DeductionPaymentsDto
    {   
        public Currency Currency { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountPaidInDollars { get; set; }
        public DeductionPaymentStatus Status { get; set; }
        public SourceDeductionPayment Origin { get; set; }

        public DeductionDetails DeductionDetails { get; set; } = new();
    }

    public class DeductionDetails
    {
        public Guid PayrollId { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly StartDate { get; set; }
    }
}