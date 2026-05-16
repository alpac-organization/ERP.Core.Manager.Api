using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Domain.Enums;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GenerateDocumentToCollaboratorQuery: BaseRequest, IRequest<byte[]>
    {
        public DocumentType DocumentType { get; set; }
        public string? IdentificationNumber { get; set; }
    }
}