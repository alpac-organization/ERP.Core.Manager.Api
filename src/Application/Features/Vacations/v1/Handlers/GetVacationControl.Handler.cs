using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationControlHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetVacationControlQuery, PagedResponse<VacationAccruals>>(_unitOfWork, _errorManager)
    {
       public override async Task<PagedResponse<VacationAccruals>> Handle(GetVacationControlQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            bool hasPermission = access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Supervisor;

            if (hasPermission)
            {
                switch(request.Type)
                {
                    case VacationReportType.VacationAccrual :
                    {
                        var baseQuery = _unitOfWork.Vacations.Entities
                            .Include(vac => vac.Collaborator)
                                .ThenInclude(col => col.WorkingInformation)
                                    .ThenInclude(col => col.WorkArea)
                            .AsNoTracking();

                        if (request.WorkAreaId.HasValue)
                        {
                            baseQuery = baseQuery.Where(vac => vac.Collaborator.WorkingInformation.WorkArea.CatalogId == request.WorkAreaId);
                        }

                        var totalRecords = await baseQuery.CountAsync(cancellationToken);

                        var vacations = await baseQuery
                            .OrderBy(x => x.Collaborator.FirstLastname) 
                            .Skip((request.PageNumber - 1) * request.PageSize)
                            .Take(request.PageSize)
                            .ToListAsync(cancellationToken);

                        var VacationAccruals = _mapper.Map<List<VacationAccruals>>(vacations);
                        
                        return new PagedResponse<VacationAccruals>(
                            VacationAccruals, 
                            request.PageNumber, 
                            request.PageSize, 
                            totalRecords
                        );  
                    }
                    default:
                    {
                        return _errorManager.ThrowBadRequest<PagedResponse<VacationAccruals>>("Este tipo no se encuentra disponible", "ERP:02");   
                    }
                }
            }
            else
            {
                return _errorManager.ThrowBadRequest<PagedResponse<VacationAccruals>>("No tienes acceso para ver este informe control vacaciones", "ERP:01");
            }       
        }
    }
}