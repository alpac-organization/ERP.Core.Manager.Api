using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Application.Commons.Interfaces;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Handlers
{
    public class GetCostCenterHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetCostCentersQuery, List<CostCenterDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<CostCenterDto>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var isAdministrator = access.Role?.RoleType == RoleType.Administrator;

            var costCentersQuery = _unitOfWork.CostCenters.Entities
                .AsNoTracking()
                .Where(cost => cost.IsActive)
                .Where(cost => cost.WorkArea.CompanyId == request.CompanyId)
                .Where(cost => cost.WorkArea.IsActive);

            // ---------- Sucursal ----------
            // Solo centros de costo asociados a una sucursal (navegacion por branch)
            costCentersQuery = costCentersQuery
                .Where(cost => cost.BranchId != null);

            if (!isAdministrator)
            {
                costCentersQuery = costCentersQuery
                    .Where(cost => cost.BranchId == access.Profile.BranchId);
            }
            else if (request.BranchId.HasValue)
            {
                costCentersQuery = costCentersQuery
                    .Where(cost => cost.BranchId == request.BranchId.Value);
            }

            // ---------- Operacional (disponible para administrador y manager) ----------
            if (request.OnlyOperational.HasValue)
            {
                costCentersQuery = costCentersQuery
                    .Where(cost => cost.IsOperational == request.OnlyOperational.Value);
            }

            var costCenters = await costCentersQuery.ToListAsync(cancellationToken);

            return _mapper.Map<List<CostCenterDto>>(costCenters);
        }
    }
}