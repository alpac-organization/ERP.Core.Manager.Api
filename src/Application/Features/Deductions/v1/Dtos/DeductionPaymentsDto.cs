using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos
{
    public class DeductionPaymentsDto
    {   
        public int? NumberFortnights { get; set; }
        public int? NumberFortnightsPaid { get; set; }

        public decimal? FortnightlyAmount { get; set; } 
        public decimal? FortnightlyAmountInDollars { get; set; } 

        public decimal? TotalBalance { get; set; }
        public decimal? TotalBalanceInDollars { get; set; }

        public decimal? AmountPaid { get; set; }
        public decimal? AmountPaidInDollars { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalAmountInDollars { get; set; }
    }
}