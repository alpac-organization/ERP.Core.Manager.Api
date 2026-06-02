using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Branches.v1.Commands
{
    public class RegisterBranchCommand: BaseRequest, IRequest<bool>
    {
        public string? BrachName { get; set; }
        public string? BranchCode { get; set; }
        public string? Descripcion { get; set; }
    }
}