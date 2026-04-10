using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Vacations.v1.Queries;

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
        [HttpGet("companies/{companie_id}/modules/{module_code}/vacations")]      
        [ProducesResponseType(typeof(VacationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> GetVacationControl([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;



            return Ok();
        }
    }
}