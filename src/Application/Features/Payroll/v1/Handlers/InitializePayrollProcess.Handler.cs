using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Domain.Enums;
using Microsoft.Extensions.Logging;
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class InitializePayrollProcessHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, 
        ILogger<InitializePayrollProcessHandler> _logger): AlpacBaseHandler<InitializePayrollProcessCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(InitializePayrollProcessCommand request, CancellationToken cancellationToken)
        {

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
                TotalToPay = 0.0m
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
                .ToListAsync(cancellationToken);

            switch (request.Type)
            {
                case PayrollType.Ordinary:
                {
                    //Recorremos todos los colaboradores
                    foreach(var collaborator in collaborators)
                    {
                        
                    }

                    break;
                }
                default:
                {
                    return _errorManager.ThrowBadRequest<bool>("Error la crear esta nomina, el tipo de nomina no es valido", "ERP:01");    
                }
            }



            await _unitOfWork.Payrolls.InitializePayroll(newPayroll);

            return true;
        }
    }
}   