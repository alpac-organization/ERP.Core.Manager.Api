using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class ClosePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<ClosePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(ClosePayrollProcessCommand request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("Solo los administradores pueden cerrar el proceso de nomina", "ERP:001");
            }

            var branch = await _unitOfWork.Branches.Entities
                .Where(branch => branch.Id == request.BranchId && branch.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (branch is null)
            {
                return _errorManager.ThrowBadRequest<bool>("La sucursal seleccionada no estas asociado a este compañia", "ERP:BrachNotFound");
            }

            //Iniciando periodo de cierre de prueba

            var payroll = await _unitOfWork.Payrolls.Entities 
                .Where(pay => pay.BranchId == request.BranchId && pay.PayrollType == request.PayrollType)
                .Where(pay => pay.Id == request.PayrollId)
                .Include(pay => pay.OrdinaryPayrolls)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return  _errorManager.ThrowBadRequest<bool>("Esta nomina no se encuentra en curso o no existe", "ERP:02");
            }

            payroll.Status = PayrollStatus.Closed;

            foreach (var collaborators in payroll.OrdinaryPayrolls)
            {
                //Todo los colaboradores actuales
            }

            return true;
        }
    }
}   