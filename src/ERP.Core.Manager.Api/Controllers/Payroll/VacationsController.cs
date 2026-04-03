using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class VacationsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Vacaciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/vacations")]      
        [ProducesResponseType(typeof(VacationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<VacationDto> GetVacationBalanceAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetVacationBalanceQuery()
            {
                CompanyId = companie_id,
                IdentificationNumber = identification_number,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        [Tags("Vacaciones")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/vacations")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CreatedResult> CreateVacationRequestRecordAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number, 
            [FromBody] CreateVacationRequestRecordCommand Payload 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new CreateVacationRequestRecordCommand()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                IdentificationNumber = identification_number,
                Description = Payload.Description,
                EndDate = Payload.EndDate,
                StartDate = Payload.StartDate,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Created(string.Empty, null);
        }

        [Tags("Vacaciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/vacations")]      
        [ProducesResponseType(typeof(List<VacationRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<VacationRequestDto>> GetVacationRequestsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromQuery] string? identification_number, 
            [FromQuery] int page_size = 10, 
            [FromQuery] int page_number = 1, 
            [FromQuery] VacationRequestStatus? status = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetVacationRequestQuery()
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
    }
}