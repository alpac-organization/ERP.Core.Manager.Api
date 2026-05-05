using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class ObtainPayrollPeriodsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<ObtainPayrollPeriodsQuery, List<PayrollPeriodDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<PayrollPeriodDto>> Handle(ObtainPayrollPeriodsQuery request, CancellationToken cancellationToken)
        {

            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var payrolls = await _unitOfWork.Payrolls.Entities
                .Include(payroll => payroll.Branch)
                .Where(payroll => payroll.BranchId == request.BrachId && payroll.PayrollType == request.Type)
                .Where(payroll => payroll.Status == PayrollStatus.Closed)
                .OrderByDescending(p => p.EndDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var periods = _mapper.Map<List<PayrollPeriodDto>>(payrolls);

            return periods;
        }
    }
}   