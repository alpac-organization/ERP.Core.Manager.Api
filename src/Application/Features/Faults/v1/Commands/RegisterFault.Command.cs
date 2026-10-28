using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using MediatR;

namespace ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands
{
   public class RegisterFaultCommand : BaseRequest, IRequest<bool>
   {
      public List<Fault> Faults { get; set; } = [];
   }

   public class Fault
   {
      public Guid CollaboratorId { get; set; }
      public Guid FaultCatalogId { get; set; }
      public DateOnly OccurrenceDate { get; set; }
      public FaultSource Source { get; set; }
      public Guid? ParentFaultId { get; set; }
      public List<EvidenceInput> Evidences { get; set; } = [];
   }
}