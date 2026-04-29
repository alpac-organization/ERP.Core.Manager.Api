using AutoMapper;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Bases;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;

namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Handlers
{
   public class GetReportsByTypeHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager) : AlpacBaseHandler<GetReportsByTypeQuery, ReportsDto>(_unitOfWork, _errorManager)
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

                default:
                {
                    return _errorManager.ThrowBadRequest<ReportsDto>("Este tipo de reporte no se encuentra disponible", "ERP:01");   
                }
            }

            return reportDto;
        }
    } 
}