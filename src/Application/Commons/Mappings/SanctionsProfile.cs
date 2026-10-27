using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
   public class SanctionsProfile : Profile
   {
      public SanctionsProfile()
      {
         CreateMap<DisciplinarySanction, SanctionDto>()
            .ForMember(dest => dest.SanctionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Fault.CollaboratorId))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src =>
               ManagerUtils.FromSliceToCollaboratorFullname(src.Fault.Collaborator)));

         CreateMap<DisciplinarySanction, SanctionDetailDto>()
            .ForMember(dest => dest.SanctionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.Fault.CollaboratorId))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src =>
               ManagerUtils.FromSliceToCollaboratorFullname(src.Fault.Collaborator)))
            .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Fault.Collaborator.IdentificationNumber))
            .ForMember(dest => dest.FaultCatalog, opt => opt.MapFrom(src => src.Fault.FaultCatalog))
            .ForMember(dest => dest.OccurrenceDate, opt => opt.MapFrom(src => src.Fault.OccurrenceDate))
            .ForMember(dest => dest.FaultSeverity, opt => opt.MapFrom(src => src.Fault.Severity))
            .ForMember(dest => dest.Evidences, opt => opt.MapFrom(src => DisciplinaryEvidenceHelper.Deserialize(src.Evidence)))
            .ForMember(dest => dest.IssuedByUser, opt => opt.Ignore());
      }
   }

   public static class SanctionMapper
   {
      public static DisciplinarySanction ToDisciplinarySanctionEntity(
         this RegisterSanctionCommand command,
         string evidenceJson = "{}")
      {
         return new DisciplinarySanction
         {
            // PayrollId se enlaza después con efectos de nómina
            FaultId = command.FaultId,
            SanctionType = command.SanctionType,
            SanctionStatus = SanctionStatus.Applied,
            SuspensionStartDate = command.SanctionType == SanctionType.Suspension ? command.SuspensionStartDate : null,
            SuspensionEndDate = command.SanctionType == SanctionType.Suspension ? command.SuspensionEndDate : null,
            Reason = command.Reason,
            IssuedBy = command.UserId,
            Evidence = string.IsNullOrWhiteSpace(evidenceJson) ? "{}" : evidenceJson
         };
      }
   }
}
