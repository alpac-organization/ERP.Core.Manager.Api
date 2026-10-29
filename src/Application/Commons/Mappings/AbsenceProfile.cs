using AutoMapper;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class AbsenceProfile : Profile
    {
        public AbsenceProfile()
        {
            CreateMap<Absence, AbsenceDto>()
                .ForMember(dest => dest.AbsenceId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.CollaboratorId))
                .ForMember(dest => dest.Fullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)))
                .ForMember(dest => dest.IdentificationNumber, opt => opt.MapFrom(src => src.Collaborator.IdentificationNumber))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Collaborator.WorkingInformation.Branch.BranchName))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source));

            CreateMap<Absence, AbsenceDetailDto>()
                .IncludeBase<Absence, AbsenceDto>()
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.RegisteredBy, opt => opt.MapFrom(src => src.RegisteredBy))
                .ForMember(dest => dest.ReviewedBy, opt => opt.MapFrom(src => src.ReviewedBy))
                .ForMember(dest => dest.ReviewedAt, opt => opt.MapFrom(src => src.ReviewedAt))
                .ForMember(dest => dest.CancellationReason, opt => opt.MapFrom(src => src.CancellationReason))
                .ForMember(dest => dest.SalaryDeduction, opt => opt.MapFrom(src => src.SalaryDeduction))
                .ForMember(dest => dest.SeventhDayDeduction, opt => opt.MapFrom(src => src.SeventhDayDeduction))
                .ForMember(dest => dest.TravelExpensesDeduction, opt => opt.MapFrom(src => src.TravelExpensesDeduction))
                .ForMember(dest => dest.TotalDeduction, opt => opt.MapFrom(src => src.TotalDeduction))
                .ForMember(dest => dest.PayrollId, opt => opt.MapFrom(src => src.PayrollId))
                .ForMember(dest => dest.OrdinaryPayrollId, opt => opt.MapFrom(src => src.OrdinaryPayrollId))
                .ForMember(dest => dest.DisciplinaryFaultId, opt => opt.MapFrom(src => src.DisciplinaryFaultId))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt));
        }
    }
}
