using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class GetDeductionDetailsHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetDeductionDetailsQuery, DeductionDetailsDto>(_unitOfWork, _errorManager)
    {
        public override async Task<DeductionDetailsDto> Handle(GetDeductionDetailsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var recordDetails = await _unitOfWork.Deductions.Entities
                .Include(deduction => deduction.Collaborator)
                .Where(deduction => deduction.Id == request.DeductionId)
                .Where(deduction => deduction.Collaborator.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return _mapper.Map<DeductionDetailsDto>(recordDetails);
        }
    }
}