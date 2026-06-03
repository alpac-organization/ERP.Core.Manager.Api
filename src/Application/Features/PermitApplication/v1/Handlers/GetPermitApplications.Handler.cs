using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Application.Commons.Bases;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;

//✅Obtener solicitudes de permisos realizadas por empresas.
namespace ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Handlers
{
    public class GetPermitApplicationsRequestHandler(IUnitOfWork _unitOfWork, IMapper  _mapper, IErrorManager _errorManager) : AlpacBaseHandler<GetPermitApplicationsQuery, PagedResponse<PermitApplicationDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<PagedResponse<PermitApplicationDto>> Handle(GetPermitApplicationsQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if(!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            var permitQuery = _unitOfWork.PermitApplications.Entities
                .Include(permit => permit.Payroll)
                .Include(permit => permit.Collaborator)
                .Where(permit => permit.Collaborator.CompanyId == request.CompanyId)
                .AsNoTracking();

            if (request.PayrollId.HasValue)
            {
                permitQuery = permitQuery
                    .Where(permit => permit.PayrolId == request.PayrollId);
            }

            if (!string.IsNullOrEmpty(request.IdentificationNumber))
            {
                permitQuery = permitQuery
                    .Where(permit => permit.Collaborator.IdentificationNumber == request.IdentificationNumber);
            }

            if (request.Type.HasValue)
            {
                permitQuery = permitQuery
                    .Where(permit => permit.Type == request.Type);
            }

            if (request.Status.HasValue)
            {
                permitQuery = permitQuery
                    .Where(permit => permit.Status == request.Status);
            }

            //✅Contar el total de solicitudes en base a los filtros
            var totalPermitApplications = await permitQuery.CountAsync(cancellationToken);
            
            //Obtener todos los elementos con filtros aplicados
            var records = await permitQuery
                .OrderByDescending(info => info.CreatedAt) 
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var recordsMapped = _mapper.Map<List<PermitApplicationDto>>(records);

            return new PagedResponse<PermitApplicationDto>(
                recordsMapped,
                request.PageNumber,
                request.PageSize,
                totalPermitApplications
            );
        }
    }
}