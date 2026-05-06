using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class VacationsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Vacaciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/vacations")]      
        [ProducesResponseType(typeof(VacationDto), StatusCodes.Status200OK)]
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
        [HttpPut("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/vacations")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> UpdateVacationBalanceAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number, 
            [FromBody] UpdateVacationBalanceCommand Payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new UpdateVacationBalanceCommand()
            {
                CompanyId = companie_id,
                IdentificationNumber = identification_number,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                VacationBalance = Payload.VacationBalance,
                VacationId = Payload.VacationId
            });

            return Ok();
        }

        [Tags("Vacaciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/vacations")]      
        [ProducesResponseType(typeof(PagedResponse<VacationControlDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponse<VacationControlDto>> GetVacationControl([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromQuery] DateTime start_date,
            [FromQuery] DateTime end_date,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            return await _mediator.Send(new GetVacationControlQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                EndDate = end_date,
                StartDate = start_date,
                UserId = Guid.Parse(userIdStr ?? ""),
                PageNumber = page_number,
                PageSize = page_size
            });
        }
    }
}