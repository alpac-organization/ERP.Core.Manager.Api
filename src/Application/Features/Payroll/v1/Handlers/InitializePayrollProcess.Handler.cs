using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
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

            if (access.Role!.RoleType != RoleType.Administrator)
            {
                return _errorManager.ThrowBadRequest<bool>("Solo los administradores pueden aperturar el ciclo de la nomina", "ERP:001");
            }

            var branch = await _unitOfWork.Branches.Entities
                .Where(branch => branch.Id == request.BranchId && branch.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);


            if (branch is null)
            {
                return _errorManager.ThrowBadRequest<bool>("La sucursal seleccionada no estas asociado a este compañia", "ERP:BrachNotFound");
            }

            var lastPayroll = await _unitOfWork.Payrolls.Entities 
                .Where(payroll => payroll.BranchId == request.BranchId)
                .Include(payroll => payroll.Branch)
                    .ThenInclude(branch => branch.Company)
                .Where(payroll => payroll.Branch.Company.Id == request.CompanyId)
                .Where(payroll => payroll.Status == PayrollStatus.Closed)
                .Where(payroll => payroll.PayrollType == request.Type)
                .FirstOrDefaultAsync(cancellationToken);

            DateTime startDate;
            DateTime endDate;

            if (lastPayroll == null || !lastPayroll.EndDate.HasValue)
            {
                DateTime hoy = DateTime.Now.Date;
                if (hoy.Day <= 15)
                {
                    startDate = new DateTime(hoy.Year, hoy.Month, 1);
                    endDate = new DateTime(hoy.Year, hoy.Month, 15);
                }
                else
                {
                    startDate = new DateTime(hoy.Year, hoy.Month, 16);
                    endDate = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(1).AddDays(-1);
                }
            }
            else
            {
                DateTime lastEnd = lastPayroll.EndDate.Value.Date;

                if (lastEnd.Day == 15)
                {
                    startDate = new DateTime(lastEnd.Year, lastEnd.Month, 16);
                    endDate = new DateTime(lastEnd.Year, lastEnd.Month, 1).AddMonths(1).AddDays(-1);
                }
                else
                {
                    startDate = new DateTime(lastEnd.Year, lastEnd.Month, 1).AddMonths(1);
                    endDate = new DateTime(startDate.Year, startDate.Month, 15);
                }
            }

            var newPayroll = new Database.Domain.Entities.Payrolls.Payroll()
            {   Id = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                Status = PayrollStatus.Progress,
                PayrollType = request.Type,
                BranchId = request.BranchId
            };

            await _unitOfWork.Payrolls.InitializePayroll(newPayroll);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            switch (request.Type)
            {
                case PayrollType.Ordinary:
                {
                    var collaborators = await _unitOfWork.Collaborators.Entities
                        .Include(c => c.Salaries
                            .Where(s => s.EndDate == null && s.SalaryType == SalaryType.Fixed)
                        )
                        .Include(c => c.WorkingInformation)
                        .Where(c => c.CompanyId == request.CompanyId)
                        .Where(c => c.Status != CollaboratorStatus.Inactive)
                        .Where(c => c.Salaries
                            .Any(s => s.EndDate == null && s.SalaryType == SalaryType.Fixed)
                        )
                        .Where(c => c.WorkingInformation.CompanyBranchId == request.BranchId)
                        .ToListAsync(cancellationToken);

                    foreach(var collaborator in collaborators)
                    {

                        var IncomeTaxAccrualInformation = await _unitOfWork.IncomeTaxAccrual.Entities
                            .Include(tax => tax.Collaborator)
                                .ThenInclude(col => col.WorkingInformation)
                                    .ThenInclude(work => work.BranchInfo)
                            .Where(
                                col => 
                                col.CollaboratorId == collaborator.Id && 
                                col.Collaborator.CompanyId == request.CompanyId && 
                                col.Collaborator.WorkingInformation.BranchInfo.Id == request.BranchId
                            )
                            .FirstOrDefaultAsync(cancellationToken);

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

            return true;
        }
    }
}   