using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using AutoMapper;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GetCollaboratorsAvailableHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetCollaboratorsAvailableQuery, List<GetCollaboratorDto>>
    {
        public async Task<List<GetCollaboratorDto>> Handle(GetCollaboratorsAvailableQuery request, CancellationToken cancellationToken)
        {
            var collaborators = await _unitOfWork.Collaborators.Entities.Where(collaborator => collaborator.Status != CollaboratorStatus.Inactive)
                .Where(collaborator => string.IsNullOrEmpty(request.IdentificationNumber) || collaborator.IdentificationNumber == request.IdentificationNumber)
                .Where(collaborator => request.BranchSubCatalogId == 0 || collaborator.WorkingInformation.BranchId == request.BranchSubCatalogId)
                .Where(collaborator => request.AreaSubCatalogId == 0 || collaborator.WorkingInformation.WorkAreaId == request.AreaSubCatalogId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<GetCollaboratorDto>>(collaborators) ?? [];
        }
    }
}