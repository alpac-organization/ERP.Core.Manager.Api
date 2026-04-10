using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationControlHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetVacationControl, List<VacationControlDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<VacationControlDto>> Handle(GetVacationControl request, CancellationToken cancellationToken)
        {
            //Comenzar logica para mapeo de datos
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if(access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Supervisor || access.Role!.RoleType == RoleType.Supervisor)
            {
                var permitApplications = await _unitOfWork.PermitApplications.Entities
                    .Include(prt => prt.Collaborator)
                    .Where(prt => prt.Type == PermitApplicationType.DonatedVacations || prt.Type == PermitApplicationType.Vacation)
                    .ToListAsync(cancellationToken);

                return [];
            }
            else
            {
                return _errorManager.ThrowBadRequest<List<VacationControlDto>>("No tienes acceso para ver este informe control vacaciones", "ERP:01");
            }       
        }
    }
}