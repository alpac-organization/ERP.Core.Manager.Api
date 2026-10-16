using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetFaultCatalogQuery: IRequest<PagedResponse<FaultCatalogDto>>
   {
      public Guid CompanyId { get; set; }
      public Guid ModuleCode { get; set; }
      public int PageSize { get; set; }
      public int PageNumber { get; set; }
   }
}