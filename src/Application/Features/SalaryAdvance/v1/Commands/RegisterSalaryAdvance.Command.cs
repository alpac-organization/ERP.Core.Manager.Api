using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.SalaryAdvance.v1.Commands
{
    public class RegisterSalaryAdvanceCommand: BaseRequest, IRequest<bool>
    {
        public Guid CollaboratorId { get; set; }
        public string? Description { get; set; }
        public DeductionType DeductionType { get; set; }

        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
    }
}