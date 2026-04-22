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
        
        public OtherDeductions? OtherDeductions { get; set; } = new();
    }

    public class OtherDeductions
    {
        public int NumberOfFortnights { get; set; }
        public decimal TotalAmount { get; set; }
    }
}