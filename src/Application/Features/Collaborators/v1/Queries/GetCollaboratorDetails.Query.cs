using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorDetailsQuery : IRequest<CollaboratorDetailsDto>
    {
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string? ModuleCode { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}