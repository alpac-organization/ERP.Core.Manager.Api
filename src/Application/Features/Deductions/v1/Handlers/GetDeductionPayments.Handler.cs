using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class GetDeductionsPaymentsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetDeductionPaymentsQuery, PagedResponseDeduction<DeductionPaymentsDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponseDeduction<DeductionPaymentsDto>> Handle(GetDeductionPaymentsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var paymentsRecord = await _unitOfWork.DeductionPaymentHistories.Entities
                .Include(his => his.Payroll)
                .Where(his => his.DeductionId == request.DeductionId)
                .OrderBy(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            int totalDeductions = await _unitOfWork.DeductionPaymentHistories.Entities.CountAsync(cancellationToken);

            var deductionsMapped = _mapper.Map<List<DeductionPaymentsDto>>(paymentsRecord);

            return new PagedResponseDeduction<DeductionPaymentsDto>(
                deductionsMapped,
                request.PageNumber,
                request.PageSize,
                totalDeductions
            );           
        }
    }
}