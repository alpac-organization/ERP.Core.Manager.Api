using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class UpdateVacationBalanceHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<UpdateVacationBalanceCommand,bool>(_unitOfWork, _errorManager)
    {
       public override async Task<bool> Handle(UpdateVacationBalanceCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            bool hasPermission = access.Role!.RoleType == RoleType.Administrator;

            if (hasPermission)
            {
                var vacationControl = await _unitOfWork.Vacations.Entities
                    .Include(vac => vac.Collaborator)
                    .Where(vac => vac.Collaborator.IdentificationNumber == request.IdentificationNumber && vac.Id == request.VacationId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (vacationControl is null)
                {
                    return _errorManager.ThrowBadRequest<bool>("Este Colaborado no cuenta con control de vacaciones", "ERP:01");
                }

                vacationControl.AvailableVacations = request.VacationBalance;
                vacationControl.EnjoyedVacation = 0;
                vacationControl.GeneredVacation = request.VacationBalance;

                await _unitOfWork.Vacations.UpdateAsync(vacationControl);
                await _unitOfWork.SaveChangesAsync(cancellationToken);  

                return true;
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("No tienes acceso para ver este informe control vacaciones", "ERP:01");
            }       
        }
    }
}