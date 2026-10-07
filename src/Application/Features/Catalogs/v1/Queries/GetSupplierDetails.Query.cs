using MediatR;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries
{
    public class GetSupplierDetailsQuery: BaseRequest, IRequest<SupplierInformationDto>
    {
        public Guid SupplierId { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public string? Code { get; set; }
        public Guid? UnitMeasureId { get; set; }
    }
}