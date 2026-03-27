using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries
{
    public class GetCollaboratorsAvailableQuery : IRequest<List<GetCollaboratorDto>>
    {
        public string? IdentificationNumber { get; set; }
        public string? BranchSubCatalogId  { get; set; }
        public string? Status { get; set; }
    }
}