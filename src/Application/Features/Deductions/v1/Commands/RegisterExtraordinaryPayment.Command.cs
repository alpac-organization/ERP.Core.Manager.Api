using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands
{
    public class RegisterExtraordinaryPaymentCommand: BaseRequest, IRequest<bool>
    {
        public Guid DeductionId  { get; set; }
        public Currency Currency { get; set; }
        
        public decimal AmountPaid { get; set; }
        public DeductionType DeductionType { get; set; }
    }
}