using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries
{

   public class GetPermitTypePoliciesQuery: BaseRequest, IRequest<List<PermitTypePolicyDto>>
   {
      public bool OnlyActive {get; set;} = true;
   }
}