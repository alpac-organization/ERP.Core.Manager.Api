using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class CheckIfThereIsPayrollInProgressHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<CheckIfThereIsPayrollInProgressQuery, CheckPayrollDto>(_unitOfWork, _errorManager)
    {
        public override async Task<CheckPayrollDto> Handle(CheckIfThereIsPayrollInProgressQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var isTherePending = await _unitOfWork.Payrolls.Entities
                .Include(payroll => payroll.Branch)
                    .ThenInclude(branch => branch.Company)
                .Where(payroll => payroll.Branch.CompanyId == request.CompanyId)
                .Where(payroll => payroll.Status == PayrollStatus.Progress)
                .Where(payroll => payroll.PayrollType == request.PayrollType)
                .Where(payroll => payroll.BranchId == request.BranchId)
                .FirstOrDefaultAsync(cancellationToken);

            if (isTherePending is null)
            {
                return new()
                {
                    ExistPayrollInProgress = false,
                    PayrollId = null
                };
            }
            else
            {
                return new ()
                {
                    ExistPayrollInProgress = true,
                    PayrollId = isTherePending.Id
                };
            }
        }
    }
}