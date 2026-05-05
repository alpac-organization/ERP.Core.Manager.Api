using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.Vacations.v1.Handlers
{
    public class GetVacationControlHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper): AlpacBaseHandler<GetVacationControlQuery, PagedResponse<VacationControlDto>>(_unitOfWork, _errorManager)
    {
       public override async Task<PagedResponse<VacationControlDto>> Handle(GetVacationControlQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse!; 
            }

            bool hasPermission = access.Role!.RoleType == RoleType.Administrator || access.Role!.RoleType == RoleType.Supervisor;

            if (hasPermission)
            {
                var query = _unitOfWork.PermitApplications.Entities
                    .Include(prt => prt.Collaborator)
                        .ThenInclude(c => c.WorkingInformation)
                            .ThenInclude(w => w.WorkPosition)
                    .Where(prt => prt.Collaborator.CompanyId == request.CompanyId)
                    .Where(prt => prt.Type == PermitApplicationType.DonatedVacations || prt.Type == PermitApplicationType.Vacation)
                    .Where(prt => prt.Status == PermitApplicationStatus.Approved)
                    .AsNoTracking();

                if (request.StartDate.HasValue)
                {
                    query = query.Where(prt => prt.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    var endLimit = request.EndDate.Value.Date.AddDays(1);
                    query = query.Where(prt => prt.CreatedAt < endLimit);
                }

                int totalRecords = await query.CountAsync(cancellationToken);

                var permitApplications = await query
                    .OrderByDescending(prt => prt.StartDate) 
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var information = _mapper.Map<List<VacationControlDto>>(permitApplications);

                return new PagedResponse<VacationControlDto>(
                    information,
                    request.PageNumber,
                    request.PageSize,
                    totalRecords
                );
            }
            else
            {
                return _errorManager.ThrowBadRequest<PagedResponse<VacationControlDto>>("No tienes acceso para ver este informe control vacaciones", "ERP:01");
            }       
        }
    }
}