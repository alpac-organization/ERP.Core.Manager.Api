using AutoMapper;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;

namespace ERP.Core.Manager.Api.Application.Commons.Mappings
{
    public class PayrollProfile : Profile
    {
        public PayrollProfile()
        {
            CreateMap<Payroll, PayrollPeriodDto>()
                .ForMember(dest => dest.PayrollId, src => src.MapFrom(or => or.Id))
                .ForMember(dest => dest.StartDate, src => src.MapFrom(or => or.StartDate))
                .ForMember(dest => dest.EndDate, src => src.MapFrom(or => or.EndDate))
                .ForMember(dest => dest.Type, src => src.MapFrom(or => or.PayrollType))
                .ForMember(dest => dest.BranchName, src => src.MapFrom(or => or.Branch.BranchName));

            #region Mapeo del detalles de nomina.
            CreateMap<(OrdinaryPayroll, Collaborator, WorkingInformation, SubCatalog, WorkArea), OrdinaryPayrollDetailsDto>()
                .ForMember(dest => dest.Commissions, src => src.MapFrom(or => or.Item1.Commissions))
                .ForMember(dest => dest.OrdinaryPayrollId, src => src.MapFrom(or => or.Item1.Id))
                .ForMember(dest => dest.Antique, src => src.MapFrom(or => or.Item1.Antique))
                .ForMember(dest => dest.Bonus, src => src.MapFrom(or => or.Item1.Bonus))
                .ForMember(dest => dest.Overtime, src => src.MapFrom(or => or.Item1.Bonus))
                .ForMember(dest => dest.NumberOvertime, src => src.MapFrom(or => or.Item1.NumberOvertime))
                .ForMember(dest => dest.Ir, src => src.MapFrom(or => or.Item1.Ir))
                .ForMember(dest => dest.Inss, src => src.MapFrom(or => or.Item1.Inss))
                .ForMember(dest => dest.BiweeklySalary, src => src.MapFrom(or => or.Item1.BiweeklySalary))
                .ForMember(dest => dest.TotalIncome, src => src.MapFrom(or => or.Item1.TotalIncome))
                .ForMember(dest => dest.TotalLegalDeductions, src => src.MapFrom(or => or.Item1.TotalLegalDeductions))
                .ForMember(dest => dest.Feeding, src => src.MapFrom(or => or.Item1.Feeding))
                .ForMember(dest => dest.Transport, src => src.MapFrom(or => or.Item1.Transport))
                .ForMember(dest => dest.Lodging, src => src.MapFrom(or => or.Item1.Lodging))
                .ForMember(dest => dest.GrossSalary, src => src.MapFrom(or => or.Item1.GrossSalary))
                .ForMember(dest => dest.DeductionsAdditionalData, src => src.MapFrom(or => or.Item1.DeductionsAdditionalData))
                .ForMember(dest => dest.TotalToPay, src => src.MapFrom(or => or.Item1.TotalToPay))
                .ForMember(dest => dest.Vacations, src => src.MapFrom(or => or.Item1.Vacations))
                .ForMember(dest => dest.AmountDaysVacation, src => src.MapFrom(or => or.Item1.AmountDaysVacation))
                .ForMember(dest => dest.TotalDeducctions, src => src.MapFrom(or => or.Item1.TotalDeducctions))
                .ForMember(dest => dest.TotalTravelExpenses, src => src.MapFrom(or => or.Item1.TotalTravelExpenses))
                .ForPath(
                    dest => dest.CollaboratorInformation.FullName,
                    opt => opt.MapFrom(src => string.Join(" ",
                        new[]
                        {
                            src.Item2.FirstName,
                            src.Item2.SecondName,
                            src.Item2.ThirdName,
                            src.Item2.FirstLastname,
                            src.Item2.SecondLastname
                        }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                    ))
                )
                .ForPath(dest => dest.CollaboratorInformation.IdentificationNumber, opt => opt.MapFrom(src => src.Item2.IdentificationNumber))
                .ForPath(dest => dest.CollaboratorInformation.CollaboratorCode, opt => opt.MapFrom(src => src.Item2.CollaboratorCode))

                .ForPath(dest => dest.CollaboratorInformation.EntryDate, opt => opt.MapFrom(src => src.Item3.EntryDate))
                .ForPath(dest => dest.CollaboratorInformation.BankAccount, opt => opt.MapFrom(src => src.Item3.BankAccountNumber))
                .ForPath(dest => dest.CollaboratorInformation.JobPosition, opt => opt.MapFrom(src => src.Item4.CatalogName))
                .ForPath(dest => dest.CollaboratorInformation.WorkArea, opt => opt.MapFrom(src => src.Item5.WorkAreaName))
                .ForPath(dest => dest.CollaboratorInformation.InssNumber, opt => opt.MapFrom(src => src.Item3.InssNumber));
            #endregion
        }
    }
}