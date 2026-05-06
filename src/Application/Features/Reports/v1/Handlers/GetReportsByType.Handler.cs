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

            switch(request.Type)
            {
                case ReportsType.TravelExpenses:
                {
                    //Obtener el reporte de viaticos totalde viaticos pagados en la quincena

                    break;   
                }
                case ReportsType.Accumulated:
                {
                    var incomes = await _unitOfWork.IncomeTaxAccrual.Entities
                        .Include(income => income.Payroll)
                        .Include(income => income.Collaborator)
                        .Where(income => income.PayrollId == request.PayrollId)
                        .ToListAsync(cancellationToken);
                        
                    var reportMapped = _mapper.Map<List<AccumulatedHistory>>(incomes);

                    reportDto.AccumulatedHistory = reportMapped;                    

                    break;   
                }
                case ReportsType.Incomes:
                {
                    
                    break;
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