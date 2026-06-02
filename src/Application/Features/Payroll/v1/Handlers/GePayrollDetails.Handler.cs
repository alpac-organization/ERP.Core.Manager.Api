using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;


using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Payroll.v1.Handlers
{
    public class GetPayrollDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager): AlpacBaseHandler<GePayrollDetaillsQuery, PayrollDetailsDto>(_unitOfWork, _errorManager)
    {
        public override async Task<PayrollDetailsDto> Handle(GePayrollDetaillsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var branchSelected = await _unitOfWork.Branches.Entities
                .Where(bran => bran.IsActive)
                .Where(bran => bran.Id == request.BranchId)
                .Where(bran => bran.CompanyId == request.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (branchSelected is null)
            {
                return _errorManager.ThrowBadRequest<PayrollDetailsDto>("¡Esta sucursal no esta existe!", "ERP:BranchNotFound");
            }

            var payroll = await _unitOfWork.Payrolls.Entities
                .Where(pay => pay.Id == request.PayrollId)
                .Where(pay => pay.BranchId == branchSelected.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (payroll is null)
            {
                return _errorManager.ThrowBadRequest<PayrollDetailsDto>("¡No se encontro registro de esta nomina actualmente!", "ERP:PayrollNotFound");
            }

            var payrollDetails = _unitOfWork.OrdinaryPayrolls.Entities
                .AsNoTracking()
                .Include(op => op.Collaborator)
                    .ThenInclude(col => col.WorkingInformation)
                .Where(op => op.PayrollId == payroll.Id);


            #region Filtro de nomina
            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                payrollDetails = payrollDetails
                    .Where(op => op.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.WorkAreaId.HasValue)
            {
                payrollDetails = payrollDetails
                    .Where(op => op.Collaborator.WorkingInformation.WorkAreaId == request.WorkAreaId);
            }

            if (request.WorkPositionId.HasValue)
            {
                payrollDetails = payrollDetails
                    .Where(op => op.Collaborator.WorkingInformation.WorkPositionId == request.WorkPositionId);
            }
            #endregion

            int totalRecords= await payrollDetails.CountAsync(cancellationToken);

            var records = await payrollDetails
                .OrderBy(op => op.Collaborator.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var Information = new PayrollDetailsDto ()
            {
                PayrollId = payroll.Id,
                EndDate = payroll.EndDate,
                StartDate = payroll.StartDate,
                Type = payroll.PayrollType,
                BranchName = branchSelected.BranchName,
                PageSize = request.PageSize,
                PageNumber = request.PageNumber,
                TotalItems = totalRecords
            };

            if (payroll.PayrollType == PayrollType.Ordinary)
            {
                

            }

            if(payroll.PayrollType == PayrollType.ProfessionalServices)
            {
                
            }

            return Information;
        }
    }
}   