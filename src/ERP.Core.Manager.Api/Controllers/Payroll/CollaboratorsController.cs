using MediatR;
using Microsoft.AspNetCore.Mvc;

using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;


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

            payload.UserId      = Guid.Parse(userIdStr ?? "");
            payload.ModuleCode  = module_code;
            payload.CompanyId   = companie_id;

            await _mediator.Send(payload);

            return Created();
        }

        [Tags("Colaboradores")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators")]
        [ProducesResponseType(typeof(PagedResponse<CollaboratorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponse<CollaboratorDto>> GetCollaboratorsAvailableAsync(
            [FromRoute] Guid companie_id, 
            [FromRoute] string module_code, 
            [FromQuery] Guid? area_id                 = null,
            [FromQuery] Guid? branch_id               = null,
            [FromQuery] CollaboratorStatus? status    = null,
            [FromQuery] string? identification_number = null,
            [FromQuery] int page_size   = 10,
            [FromQuery] int page_number = 1
        )   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            
            var collaborators = await _mediator.Send(new GetCollaboratorsAvailableQuery()
            {
                Status      = status,
                AreaId      = area_id,
                BranchId    = branch_id, 
                CompanyId   = companie_id,
                ModuleCode  = module_code,
                PageSize    = page_size,
                PageNumber  = page_number,
                UserId      = Guid.Parse(userIdStr ?? ""),
                IdentificationNumber = identification_number
            });

            return collaborators;
        }

        [Tags("Colaboradores")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/details")]      
        [ProducesResponseType(typeof(CollaboratorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CollaboratorDetailsDto> GetCollaboratorDetailsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number)   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var collaborator = await _mediator.Send(new GetCollaboratorDetailsQuery()
            {
                CompanyId  = companie_id,
                ModuleCode = module_code,
                UserId     = Guid.Parse(userIdStr ?? ""),
                IdentificationNumber = identification_number,
            });

            return collaborator;
        }

        [Tags("Colaboradores")] 
        [HttpPatch("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/details")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> UpdateCollaboratoInformationrAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number,
            [FromBody] UpdateCollaboratorInformationCommand Payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            Payload.CompanyId = companie_id;
            Payload.ModuleCode = module_code;
            Payload.UserId = Guid.Parse(userIdStr ?? "");
            Payload.IdentificationNumber = identification_number;

            await _mediator.Send(Payload);

            return Ok();
        }

        [Tags("Colaboradores")] 
        [HttpDelete("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<NoContentResult> DeactivateCollaboratorAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] string identification_number)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            //Desactivado por el momento

            return NoContent();
        }
    }
}