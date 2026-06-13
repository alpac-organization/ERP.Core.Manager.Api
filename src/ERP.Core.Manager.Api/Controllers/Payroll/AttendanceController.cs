using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class AttendanceController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Marcaciones")] 
        [HttpGet("companies/attendance")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceAsync([FromRoute] Guid companie_id, [FromRoute] string module_code)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;



            await _mediator.Send(new GetAttendanceQuery());

            return Ok();
        }
    }
}