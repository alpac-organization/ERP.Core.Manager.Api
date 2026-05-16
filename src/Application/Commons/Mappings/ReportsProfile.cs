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
                // NOTA: 'AccumulatedIR' y 'SalaryEarned' se mapean automáticamente si se llaman igual en Origen y Destino.
                // Si tienen nombres idénticos, puedes borrar estas dos líneas siguientes:
                .ForMember(dest => dest.AccumulatedIR, opt => opt.MapFrom(src => src.AccumulatedIR))
                .ForMember(dest => dest.SalaryEarned, opt => opt.MapFrom(src => src.SalaryEarned))
                
                // Mapeos de navegación anidados
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Payroll.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Payroll.EndDate));
                
                // Concatenación del nombre completo
                // .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => 
                //     string.Join(" ", new[] 
                //     { 
                //         src.Collaborator.FirstName, 
                //         src.Collaborator.SecondName, 
                //         src.Collaborator.FirstLastname, 
                //         src.Collaborator.SecondLastname 
                //     }.Where(s => !string.IsNullOrWhiteSpace(s)))
                //     .ToCapitalize())); // Aplicamos el ToCapitalize a la cadena final unida
        }
    }
}