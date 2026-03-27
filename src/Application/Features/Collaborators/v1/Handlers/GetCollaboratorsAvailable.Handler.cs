using MediatR;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GetCollaboratorsAvailableHandler(IUnitOfWork _unitOfWork) : IRequestHandler<GetCollaboratorsAvailableQuery, List<GetCollaboratorDto>>
    {
        public async Task<List<GetCollaboratorDto>> Handle(GetCollaboratorsAvailableQuery request, CancellationToken cancellationToken)
        {
            var collaborators = await _unitOfWork.Collaborators.Entities
            .Include(entity => entity.PersonalInformation)
            .Include(entity => entity.WorkingInformation)
            .ToListAsync(cancellationToken);

            return [];
        }
    }
}