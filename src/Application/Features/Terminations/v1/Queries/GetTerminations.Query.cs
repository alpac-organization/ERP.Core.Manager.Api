using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Queries
{
   public class GetTerminationsQuery : BaseRequest, IRequest<PagedResponse<TerminationDto>>
   {
      public string? IdentificationNumber { get; set; }
      public TerminationType? TerminationType { get; set; }
      public TerminationProcessStatus? TerminationStatus { get; set; }
      public int PageSize { get; set; } = 10;
      public int PageNumber { get; set; } = 1;
   }
}
