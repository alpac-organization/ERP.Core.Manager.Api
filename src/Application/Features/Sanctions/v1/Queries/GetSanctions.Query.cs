using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Queries
{
   public class GetSanctionsQuery : BaseRequest, IRequest<PagedResponse<SanctionDto>>
   {
      public Guid? CollaboratorId { get; set; }
      public Guid? FaultId { get; set; }
      public SanctionType? SanctionType { get; set; }
      public SanctionStatus? SanctionStatus { get; set; }
      public int PageSize { get; set; } = 10;
      public int PageNumber { get; set; } = 1;
   }
}
