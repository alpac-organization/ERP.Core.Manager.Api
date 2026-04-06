using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;

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

            var permission = await _unitOfWork.PermitApplications.Entities
                .FirstOrDefaultAsync(per => per.Id == request.PermitApplicationRequestId, cancellationToken);

            if (permission is null)
            {
                return _errorManager.ThrowBadRequest<bool>("La solicitud de permiso no existe.", "ERP:01");
            }

            if (permission.Status != PermitApplicationStatus.Pending)
            {
                return _errorManager.ThrowBadRequest<bool>(
                    $"No se puede cancelar una solicitud que ya está {permission.Status.ToString().ToLower()}.", 
                    "ERP:02"
                );
            }

            permission.Status = PermitApplicationStatus.Cancelled;

            await _unitOfWork.PermitApplications.UpdateAsync(permission);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            return true;
        }
    }
}