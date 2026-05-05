using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
namespace ERP.Core.Manager.Api.Application.Features.Deductions.v1.Handlers
{
    public class GetDeductionsHistoryHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetDeductionsHistoryQuery, PagedResponseDeduction<DeductionDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponseDeduction<DeductionDto>> Handle(GetDeductionsHistoryQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            int totalDeductions = 0;
            int totalDeductionsByCollaborator = 0;

            var baseQuery = _unitOfWork.Deductions.Entities
                .Include(deduction => deduction.Collaborator)
                .Where(deduction => deduction.Collaborator.CompanyId == request.CompanyId);


            if (request.DeductionType.HasValue)
            {
                baseQuery.Include(deduction => deduction.Type == request.DeductionType);    
            }

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                //Contabilizar los datos del colaborador
                totalDeductionsByCollaborator = await baseQuery.CountAsync(
                    deduction => deduction.Collaborator.IdentificationNumber == request.IdentificationNumber,
                    cancellationToken
                );   
            }

            totalDeductions = await baseQuery.CountAsync(cancellationToken);

            var deductions = await baseQuery
                .OrderBy(c => c.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Realizar el mapper de las deducciones para mostrar en el frotend

            var deductionsMapped = _mapper.Map<List<DeductionDto>>(deductions);

            return new PagedResponseDeduction<DeductionDto>(
                deductionsMapped,    //Deducciones
                request.PageNumber,  //Paginación
                request.PageSize,    //Limite por página
                totalDeductions,
                totalDeductionsByCollaborator
            );           
        }
    }
}