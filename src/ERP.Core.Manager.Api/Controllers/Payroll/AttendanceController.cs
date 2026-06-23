using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Attendance.v1.Queries;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class AttendanceController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Marcaciones")] 
        [HttpGet("companies/{company_id}/attendance")]
        [ProducesResponseType(typeof(PagedResponse<AttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PagedResponse<AttendanceDto>> GetAttendanceAsync(
            [FromRoute] Guid company_id,
            [FromQuery] DateTime start_date,
            [FromQuery] DateTime end_date,
            [FromQuery] string? identification_number = null,
            [FromQuery] int page_size = 10,
            [FromQuery] int page_number = 1
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            return await _mediator.Send(new GetAttendanceQuery()
            {
                IdentificationNumber = identification_number,
                StartDate = start_date,
                EndDate = end_date,
                PageSize = page_size,
                PageNumber = page_number,
                CompanyId = company_id,
            });

        }
    }
}
