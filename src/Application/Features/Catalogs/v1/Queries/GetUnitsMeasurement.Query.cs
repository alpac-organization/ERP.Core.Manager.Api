using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetUnitsMeasurementQuery : BaseRequest, IRequest<List<UnitMeasureDto>>
    {
        public UnitMeasureType? UnitMeasureType { get; set; }
    }
}