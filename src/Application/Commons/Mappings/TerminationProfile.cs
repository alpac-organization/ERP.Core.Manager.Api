using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Helpers;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
   public class TerminationProfile : Profile
   {
      public TerminationProfile()
      {
         CreateMap<CollaboratorTermination, TerminationDto>()
            .ForMember(dest => dest.TerminationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
            .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Collaborator.IdentificationNumber));

         CreateMap<CollaboratorTermination, TerminationDetailDto>()
            .IncludeBase<CollaboratorTermination, TerminationDto>()
            .ForMember(dest => dest.RegisteredByUser, opt => opt.MapFrom(src => src.RegisteredByUser))
            .ForMember(dest => dest.ClosedByUser, opt => opt.MapFrom(src => src.ClosedByUser))
            .ForMember(dest => dest.Evidences, opt => opt.MapFrom(src => EvidenceHelper.Deserialize(src.Evidence)));
      }
   }

   public static class TerminationMapper
   {
      public static CollaboratorTermination ToTerminationEntity(
         this RegisterTerminationCommand command,
         string evidenceJson = "{}")
      {
         return new CollaboratorTermination
         {
            CollaboratorId = command.CollaboratorId,
            SanctionId = command.SanctionId,
            DepartureDate = command.DepartureDate,
            NoticeDate = command.NoticeDate,
            NoticeDays = command.NoticeDays,
            Reason = command.Reason,
            TerminationType = command.TerminationType,
            TerminationStatus = TerminationProcessStatus.Pending,
            RegisteredBy = command.UserId,
            Evidence = string.IsNullOrWhiteSpace(evidenceJson) ? "{}" : evidenceJson
         };
      }
   }
}
