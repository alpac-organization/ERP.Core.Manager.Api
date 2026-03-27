using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CollaboratorsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Colaboradores")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> RegisterCollaboratorAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromBody] RegisterCollaboratorCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.ModuleCode = module_code;
            payload.CompanyId = companie_id;

            await _mediator.Send(payload);

            return Created(string.Empty, null);
        }

        [Tags("Colaboradores")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators", Name = "ObtenerColaboradores")]      
        [ProducesResponseType(typeof(List<Collaborator>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<Collaborator>> GetCollaboratorsAvailableAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, 
            [FromQuery] CollaboratorStatus status, [FromQuery] string identification_number, [FromQuery] int branch_id
        )   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            var collaborators = await _mediator.Send(new GetCollaboratorsAvailableQuery());

            return collaborators;
        }
    }
}