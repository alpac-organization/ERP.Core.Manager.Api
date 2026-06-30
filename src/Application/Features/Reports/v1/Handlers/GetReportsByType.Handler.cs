using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;

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
                default:
                    {
                        return _errorManager.ThrowBadRequest<ReportsDto>("Este tipo de reporte no se encuentra disponible", "ERP:01");
                    }
            }

            return reportDto;
        }
    }
}