using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Utils;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Handlers
{
   public class GetReportsByTypeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, IMapper _mapper) : AlpacBaseHandler<GetReportsByTypeQuery, ReportsDto>(_unitOfWork, _errorManager)
   {
      public override async Task<ReportsDto> Handle(GetReportsByTypeQuery request, CancellationToken cancellationToken)
      {
         var reportDto = new ReportsDto();

         var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

         if (!access.IsSuccess)
         {
            return access.ErrorResponse!;
         }

         switch (request.Type)
         {
            case ReportsType.VacationAccrual:
               {
                  //Logica de acumulado de vacacione
                  var queryReport = _unitOfWork.VacationAccruals.Entities
                      .Include(tax => tax.Payroll)
                      .Include(income => income.Collaborator)
                          .ThenInclude(income => income.WorkingInformation)
                      .Where(income => income.PayrollId == request.PayrollId);

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport
                         .Where(tax => tax.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  var vacationAccruals = await queryReport
                      .ToListAsync(cancellationToken);

                  var mapped = _mapper.Map<List<VacationAccrualsHistory>>(vacationAccruals);

                  reportDto.VacationAccrualsHistory = mapped;

                  return reportDto;
               }
            case ReportsType.Accumulated:
               {
                  var queryReport = _unitOfWork.IncomeTaxAccrual.Entities
                      .Include(tax => tax.Payroll)
                      .Include(income => income.Collaborator)
                          .ThenInclude(income => income.WorkingInformation)
                      .Where(income => income.PayrollId == request.PayrollId);

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport
                         .Where(tax => tax.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  if (request.AreaId.HasValue)
                  {
                     queryReport = queryReport
                         .Where(tax => tax.Collaborator.WorkingInformation.AreaId == request.AreaId);
                  }

                  var TaxIncomes = await queryReport
                      .ToListAsync(cancellationToken);

                  var mapped = _mapper.Map<List<AccumulatedHistory>>(TaxIncomes);

                  reportDto.AccumulatedHistory = mapped;

                  return reportDto;
               }
            case ReportsType.ChristmasBonusAccrual:
               {
                  //Logica de acumulado de aguinaldo

                  break;
               }
            case ReportsType.TravelExpenses:
               {
                  var queryReport = _unitOfWork.RecordsTravelExpensePayments.Entities
                      .Include(tax => tax.Payroll)
                      .Include(income => income.Collaborator)
                          .ThenInclude(income => income.WorkingInformation)
                      .Where(income => income.PayrollId == request.PayrollId);

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport
                         .Where(tax => tax.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  if (request.AreaId.HasValue)
                  {
                     queryReport = queryReport
                         .Where(tax => tax.Collaborator.WorkingInformation.AreaId == request.AreaId);
                  }

                  var records = await queryReport
                      .ToListAsync(cancellationToken);

                  var mapped = _mapper.Map<List<PaymentTravelExpensesHistory>>(records);

                  reportDto.PaymentTravelExpenses = mapped;

                  return reportDto;
               }
            case ReportsType.InssFortnightly:
            case ReportsType.InssMonthly:
               {
                  var currentPayroll = await _unitOfWork.Payrolls.Entities
                      .FirstOrDefaultAsync(p => p.Id == request.PayrollId, cancellationToken);

                  if (currentPayroll is null)
                  {
                     return _errorManager.ThrowBadRequest<ReportsDto>("Nómina no encontrada", "ERP:02");
                  }

                  var queryReport = _unitOfWork.InssAccountingInformation.Entities
                      .Include(inss => inss.Collaborator)
                          .ThenInclude(c => c.WorkingInformation)
                      .Include(col => col.Payroll)
                      .Where(inss => inss.Payroll.Branch.CompanyId == request.CompanyId)
                      .AsQueryable();

                  if (request.Type == ReportsType.InssFortnightly)
                  {
                     queryReport = queryReport.Where(inss => inss.PayrollId == request.PayrollId);
                  }
                  else if (request.Type == ReportsType.InssMonthly)
                  {
                     queryReport = queryReport.Where(inss =>
                         inss.Payroll.StartDate.Month == currentPayroll.StartDate.Month &&
                         inss.Payroll.StartDate.Year == currentPayroll.StartDate.Year &&
                         inss.Payroll.PayrollType == currentPayroll.PayrollType &&
                         inss.Payroll.BranchId == currentPayroll.BranchId);
                  }

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport.Where(inss => inss.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  if (request.AreaId.HasValue)
                  {
                     queryReport = queryReport.Where(inss => inss.Collaborator.WorkingInformation.AreaId == request.AreaId);
                  }

                  var inssRecords = await queryReport.ToListAsync(cancellationToken);

                  reportDto.InssInformation = [..inssRecords.GroupBy(x => x.CollaboratorId)
                                                    .Select(g =>
                                                    {
                                                        var collaborator = g.First().Collaborator;
                                                        return new InssInformation
                                                        {
                                                            CollaboratorCode = collaborator.CollaboratorCode ?? collaborator.IdentificationNumber,
                                                            CollaboratorFullname = ManagerUtils.FromSliceToCollaboratorFullname(collaborator),
                                                            Income = Math.Round(g.Sum(x => x.Income > 0 ? x.Income : x.InssLabor / 0.07m),
                                                            2,MidpointRounding.AwayFromZero),
                                                            Absences = g.Sum(x => x.Absence),
                                                            InssLab = g.Sum(x => x.InssLabor),
                                                            InssPatronal = g.Sum(x => x.InssPatronal),
                                                            Inatec = g.Sum(x => x.Inatec),
                                                            Total = g.Sum(x => x.Total)
                                                        };
                                                    })];
                  return reportDto;
               }
            case ReportsType.IrAndSalaryEarned:
               {
                  var payroll = await _unitOfWork.Payrolls.Entities
                      .FirstOrDefaultAsync(p => p.Id == request.PayrollId, cancellationToken);

                  if (payroll is null)
                     return _errorManager.ThrowBadRequest<ReportsDto>("Nómina no encontrada", "ERP:PayrollNotFound");


                  var queryReport = _unitOfWork.IncomeTaxAccrual.Entities
                      .Include(tax => tax.Payroll)
                      .Include(income => income.Collaborator)
                          .ThenInclude(income => income.WorkingInformation)
                      .Where(income => income.PayrollId == request.PayrollId);

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                     queryReport = queryReport.Where(x => x.Collaborator.IdentificationNumber == request.IdentificationNumber);

                  if (request.AreaId.HasValue)
                     queryReport = queryReport.Where(x => x.Collaborator.WorkingInformation.AreaId == request.AreaId);

                  var currentRecords = await queryReport.ToListAsync(cancellationToken);

                  //En el segundo periodo preparamos el dicc, para guardar lo de la primera quincena.
                  Dictionary<Guid, IncomeTaxAccrual> firstFortnightByCollaborator = [];

                  if (payroll.Period == PayrollPeriod.SecondPeriod)
                  {
                     var firstPayroll = await _unitOfWork.Payrolls.Entities
                        .Where(p => p.BranchId == payroll.BranchId)
                        .Where(p => p.PayrollType == payroll.PayrollType)
                        .Where(p => p.Period == PayrollPeriod.FirstPeriod)
                        .Where(p => p.StartDate.Year == payroll.StartDate.Year
                                 && p.StartDate.Month == payroll.StartDate.Month)
                         .FirstOrDefaultAsync(cancellationToken);

                     if (firstPayroll is not null)
                     {
                        var firstRecords = await _unitOfWork.IncomeTaxAccrual.Entities
                           .Where(x => x.PayrollId == firstPayroll.Id)
                           .ToListAsync(cancellationToken);

                        firstFortnightByCollaborator = firstRecords.ToDictionary(x => x.CollaboratorId);
                     }
                  }

                  var mapped = currentRecords.Select(record =>
                  {
                     var item = _mapper.Map<IrAndSalaryEarnedReport>(record);

                     if (payroll.Period == PayrollPeriod.FirstPeriod)
                     {
                        item.IrMonthly = null;
                        item.SalaryEarnedMonthly = null;
                     }
                     else
                     {
                        decimal firstPeriodIr = 0m;
                        decimal firstPeriodSalaryEarned = 0m;

                        if (firstFortnightByCollaborator.TryGetValue(record.CollaboratorId, out var firstRecord))
                        {
                           firstPeriodIr = firstRecord.AccumulatedIrByFornight;
                           firstPeriodSalaryEarned = firstRecord.SalaryEarnedByFornight;
                        }
                        item.IrMonthly = firstPeriodIr + item.IrFortnightly;
                        item.SalaryEarnedMonthly = firstPeriodSalaryEarned + item.SalaryEarnedFortnightly;
                     }
                     return item;

                  }).ToList();

                  reportDto.IrAndSalaryEarned = mapped;
                  return reportDto;
               }

            case ReportsType.Depreciations:
               {
                  var depreciationType = await _unitOfWork.TypesIncome.Entities
                      .Where(t => t.IncomeCode == "DEPRECIATION" && t.IsActive)
                      .FirstOrDefaultAsync(cancellationToken);

                  if (depreciationType is null)
                  {
                     return _errorManager.ThrowBadRequest<ReportsDto>("No se encontró el tipo de ingreso para depreciación", "ERP:00X");
                  }

                  var queryReport = _unitOfWork.Incomes.Entities
                      .Include(income => income.Collaborator)
                      .Where(income => income.PayrollId == request.PayrollId)
                      .Where(income => income.IncomeTypeId == depreciationType.Id);

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport
                         .Where(income => income.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  var depreciations = await queryReport.ToListAsync(cancellationToken);

                  var mapped = _mapper.Map<List<DepreciationReportDto>>(depreciations);

                  reportDto.Depreciations = mapped;

                  return reportDto;
               }
            case ReportsType.Subsidies:
               {
                  var queryReport = _unitOfWork.Subsidies.Entities
                      .Include(sub => sub.TypesSubsidy)
                      .Include(sub => sub.Payroll)
                          .ThenInclude(p => p.Branch)
                      .Include(sub => sub.Collaborator)
                          .ThenInclude(col => col.WorkingInformation)
                      .Include(sub => sub.Collaborator)
                          .ThenInclude(col => col.Salaries.Where(sal => sal.EndDate == null))
                      .AsQueryable();

                  if (request.PayrollId.HasValue && request.PayrollId.Value != Guid.Empty)
                  {
                     queryReport = queryReport.Where(sub => sub.PayrollId == request.PayrollId.Value);
                  }
                  else
                  {
                     queryReport = queryReport.Where(sub => sub.Payroll.Branch.CompanyId == request.CompanyId);
                  }
                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport
                         .Where(sub => sub.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }
                  if (request.AreaId.HasValue)
                  {
                     queryReport = queryReport
                         .Where(sub => sub.Collaborator.WorkingInformation.AreaId == request.AreaId);
                  }
                  var subsidies = await queryReport
                      .OrderByDescending(s => s.StartDate)
                      .ToListAsync(cancellationToken);

                  var mapped = subsidies.Select(s =>
                  {
                     var currentMonthlySalary = s.Collaborator?.Salaries?.FirstOrDefault()?.AmountInLocal ?? 0;
                     var dailySalary = currentMonthlySalary / 30;
                     var totalSubsidyBaseAmount = dailySalary * s.AmountDays;
                     var companyPercentage = s.Percentage / 100m;
                     var inssPercentage = 1m - companyPercentage;

                     return new SubsidyHistoryDto
                     {
                        CollaboratorCode = s.Collaborator?.CollaboratorCode,
                        CollaboratorFullName = $"{s.Collaborator?.FirstName} {s.Collaborator?.FirstLastname}",
                        AmountDays = s.AmountDays,
                        ReferenceNumber = s.ReferenceNumber,
                        TypeSubsidyName = s.TypesSubsidy?.SubsidyName,
                        StartDate = DateOnly.FromDateTime(s.StartDate),
                        EndDate = DateOnly.FromDateTime(s.EndDate),
                        Percentage = s.Percentage,
                        CompanyAssumedAmount = Math.Round(totalSubsidyBaseAmount * companyPercentage, 2),
                        InssReimbursementAmount = Math.Round(totalSubsidyBaseAmount * inssPercentage, 2)
                     };
                  }).ToList();
                  reportDto.SubsidiesHistory = mapped;
                  return reportDto;
               }
            case ReportsType.JudicialSeizures:
               {
                  var payroll = await _unitOfWork.Payrolls.Entities
                      .FirstOrDefaultAsync(p => p.Id == request.PayrollId, cancellationToken);

                  if (payroll is null)
                  {
                     return _errorManager.ThrowBadRequest<ReportsDto>("Nómina no encontrada", "ERP:PayrollNotFound");
                  }

                  var queryReport = _unitOfWork.DeductionPaymentHistories.Entities
                      .Include(p => p.Deduction)
                        .ThenInclude(c => c.Collaborator)
                           .ThenInclude(w => w.WorkingInformation)
                      .Include(p => p.Payroll)
                        .ThenInclude(b => b.Branch)
                     .Where(p => p.PayrollId == request.PayrollId)
                     .Where(p => p.Deduction.Type == DeductionType.JudicialSeizures)
                     .Where(p => p.Payroll.Branch.CompanyId == request.CompanyId)
                     .AsQueryable();

                  if (!string.IsNullOrEmpty(request.IdentificationNumber))
                  {
                     queryReport = queryReport.Where(p => p.Deduction.Collaborator.IdentificationNumber == request.IdentificationNumber);
                  }

                  if (request.AreaId.HasValue)
                  {
                     queryReport = queryReport.Where(p => p.Deduction.Collaborator.WorkingInformation.AreaId == request.AreaId);
                  }

                  var payments = await queryReport
                     .OrderBy(p => p.Deduction.Collaborator.FirstName)
                     .ToListAsync(cancellationToken);


                  reportDto.JudicialSeizures = [..payments.Select(p => new JudicialSeizures
                  {
                     PayrollId = p.PayrollId,
                     CollaboratorId = p.Deduction.CollaboratorId,
                     DeductionId = p.DeductionId,

                     CollaboratorCode = p.Deduction.Collaborator.CollaboratorCode ?? p.Deduction.Collaborator.IdentificationNumber,
                     CollaboratorFullName = ManagerUtils.FromSliceToCollaboratorFullname(p.Deduction.Collaborator),
                     IdentificationNumber = p.Deduction.Collaborator.IdentificationNumber,

                     Description = p.Deduction.Description,
                     Percentage = p.Deduction.Percentage,

                     AmountDeducted = p.AmountPaid,
                     AmountDeductedInDollars = p.AmountPaidInDollars,

                     TotalDebt = p.Deduction.TotalAmount,
                     TotalPaid = p.Deduction.AmountPaid ?? 0,
                     RemainingBalance = p.Deduction.TotalBalance ?? 0,

                     PaymentStatus = p.Status.ToString(),
                     Currency = p.Currency.ToString()
                  })];

                  return reportDto;
               }
            default:
            {
               return _errorManager.ThrowBadRequest<ReportsDto>("Este tipo de reporte no se encuentra disponible", "ERP:01");
            }
         }

         return reportDto;
      }
   }
}