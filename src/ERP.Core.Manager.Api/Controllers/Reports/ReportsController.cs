using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Reports
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class ReportsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Reportes")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/reports")]    
        [ProducesResponseType(typeof(ReportsDto), StatusCodes.Status200OK)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<ReportsDto> GetReportsByTypeAsync(
            [FromRoute] Guid companie_id, 
            [FromQuery] Guid payroll_id, 
            [FromQuery] ReportsType report_type, 
            [FromQuery] PayrollType payroll_type, 
            [FromRoute] string module_code,
            [FromQuery] string? identification_number, 
            [FromQuery] Guid? area_id 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetReportsByTypeQuery()
            {
                PayrollId            = payroll_id,
                CompanyId            = companie_id,
                PayrollType          = payroll_type,
                Type                 = report_type,
                UserId               = Guid.Parse(userIdStr ?? ""),
                IdentificationNumber = identification_number,
                AreaId               = area_id,
                ModuleCode           = module_code
            });

            return result;
        }
    }
}
