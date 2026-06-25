using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands
{
    public class DeactivateCollaboratorCommand: BaseRequest, IRequest<bool>
    {
        public string? IdentificationNumber { get; set; }
    }
}