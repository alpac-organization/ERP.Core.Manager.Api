using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;
using ERP.Core.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class GetPermitApplicationsRequestHandler(IUnitOfWork _unitOfWork, IMapper  _mapper, IErrorManager _errorManager) : AlpacBaseHandler<GetPermitApplicationQuery, List<PermitApplicationDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<PermitApplicationDto>> Handle(GetPermitApplicationQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }
            
            var query = _unitOfWork.PermitApplications.Entities
                .Include(info => info.Collaborator)
                .Where(info => info.Collaborator.CompanyId == request.CompanyId)
                .Where(info => info.Status != PermitApplicationStatus.Cancelled)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                query = query.Where(info => info.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(info => info.Status == request.Status.Value);
            }

            if (request.Type.HasValue)
            {
                query = query.Where(info => info.Type == request.Type.Value);
            }

            var items = await query
                .OrderByDescending(info => info.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<PermitApplicationDto>>(items);
        }
    }
}