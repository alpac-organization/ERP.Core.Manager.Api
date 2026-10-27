using System.Text.Json;
using AutoMapper;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using Commands = ERP.Core.Manager.Api.Application.Features.Faults.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
   public class FaultsProfile : Profile
   {
      public FaultsProfile()
      {
         CreateMap<FaultCatalog, FaultCatalogDto>()
            .ForMember(dest => dest.FaultCatalogId, opt => opt.MapFrom(src => src.Id));

         CreateMap<FaultCatalog, FaultCatalogReference>()
            .ForMember(dest => dest.FaultCatalogId, opt => opt.MapFrom(src => src.Id));

         CreateMap<DisciplinaryFault, FaultDto>()
            .ForMember(dest => dest.FaultId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.CollaboratorId))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
            .ForMember(dest => dest.FaultCatalog, opt => opt.MapFrom(src => src.FaultCatalog));

         CreateMap<DisciplinaryFault, FaultReference>()
            .ForMember(dest => dest.FaultId, opt => opt.MapFrom(src => src.Id));

         CreateMap<User, RegisterUserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Fullname))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.UserStatus.ToString()));

         CreateMap<DisciplinaryFault, FaultDetailDto>()
            .ForMember(dest => dest.FaultId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CollaboratorName, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
            .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Collaborator.IdentificationNumber))
            .ForMember(dest => dest.FaultCatalog, opt => opt.MapFrom(src => src.FaultCatalog))
            .ForMember(dest => dest.Evidences, opt => opt.MapFrom(src => MapEvidences(src.Evidence)))
            .ForMember(dest => dest.ParentFault, opt => opt.MapFrom(src => src.ParentFault))
            .ForMember(dest => dest.RegisteredBy, opt => opt.MapFrom(src => src.User));
      }

      private static List<EvidenceDto> MapEvidences(string? evidence)
      {
         if (string.IsNullOrWhiteSpace(evidence) || evidence == "{}")
            return [];

         try
         {
            return JsonSerializer.Deserialize<List<EvidenceDto>>(evidence) ?? [];
         }
         catch (JsonException)
         {
            return [];
         }
      }
   }

   public static class FaultMapper
   {
      public static DisciplinaryFault ToDisciplinaryFaultEntity(this Commands.Fault fault, FaultSeverity severity, Guid registeredBy)
      {
         var evidence = fault.Evidences is { Count: > 0 }
            ? JsonSerializer.Serialize(fault.Evidences)
            : "{}";

         return new DisciplinaryFault
         {
            CollaboratorId = fault.CollaboratorId,
            FaultCatalogId = fault.FaultCatalogId,
            OccurrenceDate = fault.OccurrenceDate,
            Severity = severity,
            Source = fault.Source,
            Status = FaultStatus.Open,
            RegisteredBy = registeredBy,
            ParentFaultId = fault.ParentFaultId,
            Evidence = evidence
         };
      }
   }
}
