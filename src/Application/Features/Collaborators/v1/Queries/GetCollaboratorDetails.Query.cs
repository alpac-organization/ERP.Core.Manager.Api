using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorDetailsQuery : BaseRequest, IRequest<CollaboratorDetailsDto>
    {
        public string? IdentificationNumber { get; set; }
    }
}