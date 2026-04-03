using MediatR;
using AutoMapper;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationRequestHandler(IUnitOfWork _unitOfWork, IMapper  _mapper, IErrorManager _errorManager) : AlpacBaseHandler<GetVacationRequestQuery, List<VacationRequestDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<VacationRequestDto>> Handle(GetVacationRequestQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }
            
            var query = _unitOfWork.VacationRequests.Entities
                .Include(info => info.Collaborator)
                .Where(info => info.Collaborator.CompanyId == request.CompanyId)
                .Where(info => info.Status != VacationRequestStatus.Cancelled)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                query = query.Where(info => info.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(info => info.Status == request.Status.Value);
            }

            var items = await query
                .OrderByDescending(info => info.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<VacationRequestDto>>(items);
        }
    }
}