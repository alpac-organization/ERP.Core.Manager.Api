using MediatR;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries
{
    public class GetReportsByTypeQuery : BaseRequest, IRequest<ReportsDto>
    {
        public ReportsType Type { get; set; }
        
        //Periodo Seleccionado.
        public Guid PayrollId { get; set; }

        public Guid? BranchId { get; set; }
        public PayrollType? PayrollType { get; set; }
    }
}