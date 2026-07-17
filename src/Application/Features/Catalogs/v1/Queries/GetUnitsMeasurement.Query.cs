using MediatR;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetUnitsMeasurementQuery : BaseRequest, IRequest<List<UnitMeasureDto>>
    {
        public UnitMeasureType? UnitMeasureType { get; set; }
    }
}