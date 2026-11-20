using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands
{
   public class RegisterTerminationCommand : BaseRequest, IRequest<bool>
   {
      public Guid CollaboratorId { get; set; }
      public TerminationType TerminationType { get; set; }
      public string Reason { get; set; } = string.Empty;
      public DateOnly DepartureDate { get; set; }
      public DateOnly? NoticeDate { get; set; }
      public int? NoticeDays { get; set; }
      public Guid? SanctionId { get; set; }
      public List<EvidenceInput> Evidences { get; set; } = [];
   }
}
