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
            
            switch(request.Type)
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

                    if (request.WorkAreaId.HasValue)
                    {
                        queryReport = queryReport
                            .Where(tax => tax.Collaborator.WorkingInformation.WorkAreaId == request.WorkAreaId);   
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

                    if (request.WorkAreaId.HasValue)
                    {
                        queryReport = queryReport
                            .Where(tax => tax.Collaborator.WorkingInformation.WorkAreaId == request.WorkAreaId);   
                    }

                    var records = await queryReport
                        .ToListAsync(cancellationToken);
                    
                    var mapped = _mapper.Map<List<PaymentTravelExpensesHistory>>(records);

                    reportDto.PaymentTravelExpenses = mapped;

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