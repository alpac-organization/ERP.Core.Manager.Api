using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class GetCollaboratorDetailsHandler(IUnitOfWork _unitOfWork, IMapper _mapper, IErrorManager _errorManager) : BaseValidatorHandler<GetCollaboratorDetailsQuery, CollaboratorDetailsDto>(_unitOfWork, _errorManager)
    {
        public override async Task<CollaboratorDetailsDto> Handle(GetCollaboratorDetailsQuery request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .AsNoTracking()
                
                .Include(c => c.PersonalInformation)

                .Include(c => c.Vacation)
                
                .Include(c => c.WorkingInformation)
                    .ThenInclude(w => w.Area)
                        .ThenInclude(a => a.CostCenters)

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
                return _errorManager.ThrowBadRequest<CollaboratorDetailsDto>("Este colaborador no existe", "ERP:001");
            }

            var mapped = _mapper.Map<CollaboratorDetailsDto>(collaborator);

            //Obtener centros de costos de la area;

            var costCenters = await _unitOfWork.CostCenters.Entities
                .Where(cos => cos.WorkAreaId == collaborator.WorkingInformation.AreaId)
                .ToListAsync(cancellationToken);

            var costCentersMapped = _mapper.Map<List<CostCenterDto>>(costCenters);

            mapped.CostCenters = costCentersMapped;

            return mapped;
        }
    }
}