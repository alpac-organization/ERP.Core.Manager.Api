using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class ModulesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Colaboradores")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> RegisterCollaboratorAsync([FromRoute] int companie_id, [FromRoute] string module_code, [FromBody] RegisterCollaboratorCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.ModuleCode = module_code;
            payload.CompanyId = companie_id;

            await _mediator.Send(payload);

            return Created(string.Empty, null);
        }

    }
}