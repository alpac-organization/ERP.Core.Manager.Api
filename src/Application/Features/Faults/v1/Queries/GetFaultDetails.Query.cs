using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries
{
   public class GetFaultDetailsQuery : BaseRequest, IRequest<FaultDetailDto>
   {
      public Guid FaultId { get; set; }
   }
}