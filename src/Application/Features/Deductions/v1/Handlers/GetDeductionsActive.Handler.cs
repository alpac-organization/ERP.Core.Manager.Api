using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class GetDeductionsActiveHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetDeductionsActiveQuery, PagedResponseDeduction<DeductionDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponseDeduction<DeductionDto>> Handle(GetDeductionsActiveQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var baseQuery = _unitOfWork.Deductions.Entities
                .Include(deduction => deduction.Collaborator)
                .Where(deduction => deduction.Collaborator.CompanyId == request.CompanyId)
                .AsNoTracking();

            if (request.DeductionStatus.HasValue)
            {
                baseQuery = baseQuery
                    .Where(deduction => deduction.Status == request.DeductionStatus);    
            }

            if (request.DeductionType.HasValue)
            {
                baseQuery = baseQuery
                    .Where(deduction => deduction.Type == request.DeductionType);    
            }

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                baseQuery = baseQuery
                    .Where(deduction => deduction.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            int totalDeductions = await baseQuery.CountAsync(cancellationToken);

            var records = await baseQuery
                .OrderBy(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var deductionsMapped = _mapper.Map<List<DeductionDto>>(records);

            return new PagedResponseDeduction<DeductionDto>(
                deductionsMapped,
                request.PageNumber,
                request.PageSize,
                totalDeductions
            );           
        }
    }
}