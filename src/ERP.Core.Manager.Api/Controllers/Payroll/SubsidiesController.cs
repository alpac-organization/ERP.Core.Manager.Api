using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class SubsidiesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Subsidio")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators/{collaborator_id}/subsidy")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterSubsidyAsync(
            [FromRoute] Guid companie_id, 
            [FromRoute] string module_code, 
            [FromRoute] Guid collaborator_id,
            [FromBody] RegisterSubsidyCommmand payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companie_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.CollaboratorId = collaborator_id;

            await _mediator.Send(payload);

            return Ok();
        }
    }
}