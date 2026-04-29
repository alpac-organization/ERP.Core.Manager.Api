using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Reports.v1.Dtos;

namespace ERP.Core.Manager.Api.Controllers.Reports
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class ReportsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Reportes")] 
        [HttpGet("companies/{companie_id}/reports")]    
        [ProducesResponseType(typeof(ReportsDto), StatusCodes.Status200OK)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<ReportsDto> GetReportsByTypeAsync([FromRoute] Guid companie_id, [FromQuery] ReportsType report_type)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetReportsByTypeQuery()
            {
                CompanyId = companie_id,
                UserId =  Guid.Parse(userIdStr ?? ""),
                Type = report_type
            });

            return result;
        }

    }
}
