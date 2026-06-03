using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;


//✅Cancelar solicitud de permisos.
namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class CancelPermitRequestHandler(IUnitOfWork _unitOfWork,  IErrorManager _errorManager) : AlpacBaseHandler<CancelPermitRequestQuery, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(CancelPermitRequestQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var requestApplication = await _unitOfWork.PermitApplications.Entities
                .Where(per => per.Id == request.PermitApplicationRequestId && per.Status == PermitApplicationStatus.Pending)
                .FirstOrDefaultAsync(cancellationToken);

            if (requestApplication is null)
            {
                return _errorManager.ThrowBadRequest<bool>("No se encontro la solicitud que desea cancelar", "ERP:01");
            }

            if (access.Role!.RoleType == RoleType.Operator)
            {
                //Reglas para operados para cancelar solicitud
                if (requestApplication.FirtsStepApproved is true)
                {
                    return _errorManager.ThrowBadRequest<bool>("Ya no puedes cancelar esta solicitud, Esperar al personal de administración", "ERP:02");
                }
                
                requestApplication.Status = PermitApplicationStatus.Cancelled;
            }
            if (access.Role!.RoleType == RoleType.Manager || access.Role.RoleType == RoleType.Administrator)
            {
                requestApplication.Status = PermitApplicationStatus.Cancelled;
            }

            await _unitOfWork.PermitApplications.UpdateAsync(requestApplication);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
}