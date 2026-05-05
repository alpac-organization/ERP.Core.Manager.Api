using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class ReporstProfile : Profile
    {
        public ReporstProfile()
        {
            CreateMap<IncomeTaxAccrual, AccumulatedHistory>()
                .ForMember(dest => dest.AccumulatedIR, src => src.MapFrom(or => or.AccumulatedIR))
                .ForMember(dest => dest.SalaryEarned, src => src.MapFrom(or => or.SalaryEarned))
                .ForMember(dest => dest.CollaboratorCode, src => src.MapFrom(or => or.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.StartDate, src => src.MapFrom(or => or.Payroll.StartDate))
                .ForMember(dest => dest.EndDate, src => src.MapFrom(or => or.Payroll.EndDate))
                .ForMember(dest => dest.CollaboratorCode, src => src.MapFrom(or => or.Collaborator.CollaboratorCode))

                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => 
                    string.Join(" ", new[] 
                    { 
                        src.Collaborator.FirstName, src.Collaborator.SecondName, src.Collaborator.FirstLastname, src.Collaborator.SecondLastname 
                    }.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.ToCapitalize()))))


            ;
        }
    }
}