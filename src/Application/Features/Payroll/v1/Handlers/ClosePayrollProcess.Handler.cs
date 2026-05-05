using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

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

            #region Verificar estado de la nomina
            var payroll = await _unitOfWork.Payrolls.Entities 
                .Where(pay => 
                    pay.BranchId == request.BranchId && pay.PayrollType == request.PayrollType
                )
                .Where(pay =>
                    pay.Id == request.PayrollId && pay.Status == PayrollStatus.Progress
                )
                .Include(pay => pay.OrdinaryPayrolls)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return  _errorManager.ThrowBadRequest<bool>("Esta nomina no se encuentra en curso o no existe", "ERP:02");
            }
            #endregion

            payroll.Status = PayrollStatus.Closed;

            await _unitOfWork.Payrolls.UpdateAsync(payroll);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            //Guardamos procesos de historiales
            var registers = payroll.OrdinaryPayrolls;

            //Realizar todo aquel, registro de deducciones y pagos realizados del colaborador
            foreach(var collaborator in registers)
            {
                //Si cuenta con deducciones registramos el pago de deducciones.



                //Registramos los pagos realizados de viaticos del colaborador.
                await _unitOfWork.AssignedTravelExpensesHistories.RegisterAssignedTravelExpensesHistory(new()
                {
                   Lodging = collaborator.Lodging,
                   Feeding = collaborator.FoodTravelAllowance,
                   Transport = collaborator.TravelExpenses,                  
                   TotalAmountPaid = collaborator.TotalTravelExpenses,
                   NumberDaysPaid = 13,
                });
            }
            
            return true;
        }
    }
}   