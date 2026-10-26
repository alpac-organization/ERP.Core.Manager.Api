using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetFaultsQuery : BaseRequest, IRequest<PagedResponse<FaultDto>>
   {
      public string? Code { get; set; }
      public FaultSeverity? Severity { get; set; }
      public FaultStatus? Status { get; set; }
      public FaultSource? Source { get; set; }
      public int PageSize { get; set; }
      public int PageNumber { get; set; }
   }
}