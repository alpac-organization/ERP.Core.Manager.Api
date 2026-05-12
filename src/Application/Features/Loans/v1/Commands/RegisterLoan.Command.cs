using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Loans.v1.Commands
{
    public class RegisterLoanCommand: BaseRequest, IRequest<bool>
    {

        public Guid PayrollId { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? Description { get; set; }


        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public int NumberFortnights { get; set; }
    }

}