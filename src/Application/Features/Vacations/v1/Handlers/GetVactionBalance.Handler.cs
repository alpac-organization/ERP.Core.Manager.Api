using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationBalanceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetVacationBalanceQuery, VacationDto>(_unitOfWork, _errorManager)
    {
        public override async Task<VacationDto> Handle(GetVacationBalanceQuery request, CancellationToken cancellationToken)
        {
            //Comenzar logica para mapeo de datos
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(c => c.CompanyId == request.CompanyId)
                .Where(c => c.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<VacationDto>("Este colaborador no existe", "ERP:001");
            }

            var vacationInformation = await _unitOfWork.Vacations.Entities
                .Where(vacation => vacation.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (vacationInformation is null)
            {
                return _errorManager.ThrowBadRequest<VacationDto>("No se encontro registro de información de vacaciones", "ERP:001");
            }

            return _mapper.Map<VacationDto>((vacationInformation, collaborator));
        }
    }
}