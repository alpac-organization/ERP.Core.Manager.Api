using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class PermitApplicationSController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Permisos")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/permit-applications")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CreatedResult> CreateVacationRequestRecordAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number, 
            [FromBody] CreatePermitApplicationCommand Payload 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new CreatePermitApplicationCommand()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                IdentificationNumber = identification_number,
                Description = Payload.Description,
                EndDate = Payload.EndDate,
                StartDate = Payload.StartDate,
                EndTime = Payload.EndTime,
                StartTime = Payload.StartTime,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Created(string.Empty, null);
        }

        [Tags("Permisos")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/permit-applications")]      
        [ProducesResponseType(typeof(List<PermitApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<PermitApplicationDto>> GetPermitApplicatiobHistoryAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, 
            [FromRoute] string identification_number,
            [FromQuery] int page_size = 10, 
            [FromQuery] int page_number = 1, 
            [FromQuery] PermitApplicationStatus? status = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetPermitApplicationHistoryQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                IdentificationNumber = identification_number,
                UserId = Guid.Parse(userIdStr ?? ""),
                PageSize = page_size,
                PageNumber = page_number,
                Status = status
            });
        }

        //Modulo de solicitudes ver todas las solicitudes
        [Tags("Vacaciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/permit-applications")]      
        [ProducesResponseType(typeof(List<PermitApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<PermitApplicationDto>> GetVacationRequestsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromQuery] string? identification_number, 
            [FromQuery] int page_size = 10, 
            [FromQuery] int page_number = 1, 
            [FromQuery] PermitApplicationStatus? status = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetPermitApplicationQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                IdentificationNumber = identification_number,
                PageSize = page_size,
                PageNumber = page_number,
                Status = status
            });            
        }

        [Tags("Vacaciones")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/vacation-requests/{vacation_request_id}/process")]      
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> ProcessVacationRequestAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] Guid vacation_request_id,
        [FromBody] ProcessPermitApplicationCommand Payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new ProcessPermitApplicationCommand()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                VacationRequestId = vacation_request_id,
                IsApproved = Payload.IsApproved
            });

            return Ok();            
        }
    }
}