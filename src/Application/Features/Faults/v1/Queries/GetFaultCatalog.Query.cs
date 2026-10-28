using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetFaultCatalogQuery : BaseRequest, IRequest<PagedResponse<FaultCatalogDto>>
   {
      public string? Code { get; set; }
      public FaultSeverity? FaultSeverity { get; set; }
      public int? PageSize { get; set; }
      public int PageNumber { get; set; }
   }
}