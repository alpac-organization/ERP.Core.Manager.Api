using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class InitializePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ICalculatorDeductions _calculatorDeductions): AlpacBaseHandler<InitializePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(InitializePayrollProcessCommand request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            //Solo Administradores puede realizar la apertura de la nomina.

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("Solo los administradores pueden aperturar el ciclo de la nomina", "ERP:001");
            }

            var lastPayroll = await _unitOfWork.Payrolls.Entities 
                .Where(payroll => payroll.CompanyId == request.CompanyId)
                .Where(payroll => payroll.Status == PayrollStatus.Progress)
                .Where(payroll => payroll.PayrollType == request.Type)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastPayroll is not null)
            {
                return _errorManager.ThrowBadRequest<bool>("No puede crear un proceso de nomina mientras existe una en proceso", "ERP:InvalidPayroll");
            }

            DateTime hoy = DateTime.Now;
            DateTime startDate = new(hoy.Year, hoy.Month, 16);
            DateTime endDate = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1).AddDays(-1);

            //Aperturamos la nomina
            var newPayroll = new Database.Domain.Entities.Payrolls.Payroll()
            {   Id = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                Status = PayrollStatus.Progress,
                PayrollType = request.Type,
                CompanyId = request.CompanyId,
                TotalToPay = 0.0m,
                BranchId = request.BranchId
            };

            //Inicializamos el proceso con exito
            await _unitOfWork.Payrolls.InitializePayroll(newPayroll);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var collaborators = await _unitOfWork.Collaborators.Entities
                .Include(c => c.Salaries.Where(s => s.EndDate == null && s.SalaryType == SalaryType.Fixed))
                .Include(c => c.WorkingInformation)
                .Where(c => c.CompanyId == request.CompanyId)
                .Where(c => c.Status != CollaboratorStatus.Inactive)
                .Where(c => c.Salaries.Any(s => s.EndDate == null && s.SalaryType == SalaryType.Fixed))
                .Where(c => c.WorkingInformation.BranchId == request.BranchId)
                .ToListAsync(cancellationToken);

            if (request.BranchId == 80)
            {
                //ALPAC EVENTUALES
            }
            else if (request.BranchId == 68)
            {
                //ALPAC Managua
                switch (request.Type)
                {
                    case PayrollType.Ordinary:
                    {
                        //Recorremos todos los colaboradores
                        foreach(var collaborator in collaborators)
                        {
                            await _calculatorDeductions.RegisterOrdinaryPayrollForCollaborator(newPayroll.Id, collaborator, cancellationToken);   
                        }

                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        break;
                    }
                    default:
                    {
                        return _errorManager.ThrowBadRequest<bool>("Error la crear esta nomina, el tipo de nomina no es valido", "ERP:01");    
                    }
                }
                
            }
            else if (request.BranchId == 67)
            {
                //ALPAC Corinto
                
            }
            else
            {
                return _errorManager.ThrowBadRequest<bool>("Esta sucursal no esta registrada en el sitema", "ERP:01");
            }

            


            return true;
        }
    }
}   