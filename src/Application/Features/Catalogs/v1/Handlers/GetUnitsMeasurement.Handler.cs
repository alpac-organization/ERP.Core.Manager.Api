using AutoMapper;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;


namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Handlers
{
    public class GetUnitsMeasurementHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : BaseValidatorHandler<GetUnitsMeasurementQuery, List<UnitMeasureDto>>(_unitOfWork, _errorManager)
    {
        public override async Task<List<UnitMeasureDto>> Handle(GetUnitsMeasurementQuery request, CancellationToken cancellationToken)
        {
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess)
            {
                return access.ErrorResponse!;
            }

            var unitsMeasurementQuery = _unitOfWork.UnitsMeasurement.Entities
                .Where(uni => uni.IsActive)
                .AsNoTracking();

            if (request.UnitMeasureType.HasValue)
            {
                unitsMeasurementQuery = unitsMeasurementQuery
                    .Where(uni => uni.Type == request.UnitMeasureType);
            }

            var unitsMeasurement = await unitsMeasurementQuery
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<UnitMeasureDto>>(unitsMeasurement);
        }
   }
}