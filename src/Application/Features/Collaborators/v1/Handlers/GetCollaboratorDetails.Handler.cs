using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;

using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GetCollaboratorDetailsHandler(IUnitOfWork _unitOfWork, IMapper _mapper, IErrorManager _erroManager) : IRequestHandler<GetCollaboratorDetailsQuery, CollaboratorDetailsDto>
    {
        public async Task<CollaboratorDetailsDto> Handle(GetCollaboratorDetailsQuery request, CancellationToken cancellationToken)
        {
            var collaborator = await _unitOfWork.Collaborators.Entities
                .AsNoTracking()
                
                .Include(c => c.PersonalInformation)

                .Include(c => c.Vacation)
                
                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.WorkArea)

                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.WorkPosition)

                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.BranchInfo)

                .Include(c => c.Salaries.Where(s => s.EndDate == null))

                .Where(c => c.CompanyId == request.CompanyId)
                .Where(c => c.IdentificationNumber == request.IdentificationNumber)
                
                .FirstOrDefaultAsync(cancellationToken);

            if(collaborator is null)
            {
                return _erroManager.ThrowBadRequest<CollaboratorDetailsDto>("Este colaborador no existe", "ERP:001");
            }

            return _mapper.Map<CollaboratorDetailsDto>(collaborator);;
        }
    }
}