using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries
{

   public class GetPermitTypePoliciesQuery: BaseRequest, IRequest<PagedResponse<PermitTypePolicyDto>>
   {
      public bool OnlyActive {get; set;} = true;

      public PermitApplicationType? PermitApplicationType { get; set; }

      public int PageNumber { get; set; } = 1;

      public int PageSize { get; set; } = 10;
   }
}