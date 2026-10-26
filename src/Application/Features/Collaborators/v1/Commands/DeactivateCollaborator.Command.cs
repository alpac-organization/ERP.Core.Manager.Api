using MediatR;
using ERP.Core.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class DeactivateCollaboratorCommand: BaseRequest, IRequest<bool>
    {
        public string? IdentificationNumber { get; set; }
    }
}