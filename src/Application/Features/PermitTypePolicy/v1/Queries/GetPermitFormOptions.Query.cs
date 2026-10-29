using MediatR;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries
{
   public class GetPermitFormOptionsQuery : BaseRequest, IRequest<PermitFormOptionsDto>
   {
   }
}