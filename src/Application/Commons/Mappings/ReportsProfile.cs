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
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)));

            CreateMap<VacationAccrual, VacationAccrualsHistory>()
                .ForMember(dest => dest.VacationBalance, opt => opt.MapFrom(src => src.AvailableVacations))
                .ForMember(dest => dest.EntryDate, opt => opt.MapFrom(src => src.Collaborator.WorkingInformation.EntryDate))
                .ForMember(dest => dest.BeginningBalance, opt => opt.MapFrom(src => src.BeginningBalance))
                .ForMember(dest => dest.FinalBalance, opt => opt.MapFrom(src => src.FinalBalance))
                .ForMember(dest => dest.EquivalesQuantity, opt => opt.MapFrom(src => src.EquivalentQuantity))
                .ForMember(dest => dest.EquivalesQuantityInDollars, opt => opt.MapFrom(src => src.EquivalentQuantityInDollars))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))

                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)));

            CreateMap<RecordsTravelExpensePayments, PaymentTravelExpensesHistory>()
                .ForMember(dest => dest.CollaboratorId, opt => opt.MapFrom(src => src.CollaboratorId))
                .ForMember(dest => dest.PayrollId, opt => opt.MapFrom(src => src.PayrollId))
                .ForMember(dest => dest.Transport, opt => opt.MapFrom(src => src.Transport))
                .ForMember(dest => dest.Lodging, opt => opt.MapFrom(src => src.Lodging))
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src =>
                    string.Join(" ", new[]
                    {
                        src.Collaborator.FirstName,
                        src.Collaborator.SecondName,
                        src.Collaborator.FirstLastname,
                        src.Collaborator.SecondLastname
                    }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    .ToCapitalize()));

            CreateMap<IncomeTaxAccrual, IrAndSalaryEarnedReport>()
                .ForMember(dest => dest.IrFortnightly, opt => opt.MapFrom(src => src.AccumulatedIrByFornight))
                .ForMember(dest => dest.SalaryEarnedFortnightly, opt => opt.MapFrom(src => src.SalaryEarnedByFornight))
                .ForMember(dest => dest.CollaboratorCode, opt => opt.MapFrom(src => src.Collaborator.CollaboratorCode))
                .ForMember(dest => dest.CollaboratorFullname, opt => opt.MapFrom(src => ManagerUtils.FromSliceToCollaboratorFullname(src.Collaborator)));
        }
    }
}