using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    // Corregido de ReporstProfile a ReportsProfile
    public class ReportsProfile : Profile
    {
        public ReportsProfile()
        {
            CreateMap<IncomeTaxAccrual, AccumulatedHistory>()
                .ForMember(dest => dest.AccumulatedIR, opt => opt.MapFrom(src => src.AccumulatedIR))
                .ForMember(dest => dest.SalaryEarned, opt => opt.MapFrom(src => src.SalaryEarned))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] 
                    { 
                        src.Collaborator.FirstName, 
                        src.Collaborator.SecondName, 
                        src.Collaborator.FirstLastname, 
                        src.Collaborator.SecondLastname 
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    .ToCapitalize()));

            CreateMap<VacationAccrual, VacationAccrualsHistory>()
                .ForMember(dest => dest.VacationBalance, opt => opt.MapFrom(src => src.AvailableVacations))
                .ForMember(dest => dest.EquivalesQuantity, opt => opt.MapFrom(src => src.EquivalentQuantity))
                .ForMember(dest => dest.EquivalesQuantityInDollars, opt => opt.MapFrom(src => src.EquivalentQuantityInDollars))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] 
                    { 
                        src.Collaborator.FirstName, 
                        src.Collaborator.SecondName, 
                        src.Collaborator.FirstLastname, 
                        src.Collaborator.SecondLastname 
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    .ToCapitalize()));
        }
    }
}