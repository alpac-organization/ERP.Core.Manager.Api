using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Dtos;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Domain.Enums;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CollaboratorsController(IMediator _mediator) : ApiControllerBase
    {
        #region Registrar Colaborador

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

        #endregion 

        [Tags("Colaboradores")] 
        [HttpPatch("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/details", Name = "UpdateCollaboratorInformation")]
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
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators", Name = "GetCollaborators")]
        [ProducesResponseType(typeof(PagedResponse<GetCollaboratorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponse<GetCollaboratorDto>> GetCollaboratorsAvailableAsync(
            [FromRoute] Guid companie_id, 
            [FromRoute] string module_code, 
            [FromQuery] CollaboratorStatus? status,
            [FromQuery] string? identification_number,
            [FromQuery] Guid? branch_id,
            [FromQuery] Guid? area_id,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var collaborators = await _mediator.Send(new GetCollaboratorsAvailableQuery()
            {
                AreaId = area_id,
                BranchId = branch_id, 
                UserId = Guid.Parse(userIdStr ?? ""),
                CompanyId = companie_id,
                ModuleCode = module_code,
                IdentificationNumber = identification_number,
                Status = status,
                PageNumber = page_number,
                PageSize = page_size
            });

            return collaborators;
        }


        [Tags("Colaboradores")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/details", Name = "CollaboratorDetails")]      
        [ProducesResponseType(typeof(CollaboratorDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CollaboratorDetailsDto> GetCollaboratorDetailsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, 
            [FromRoute] string identification_number
        )   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            
            var collaborator = await _mediator.Send(new GetCollaboratorDetailsQuery()
            {
                CompanyId = companie_id,
                IdentificationNumber = identification_number,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return collaborator;
        }

        [Tags("Colaboradores")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators/{identification_number}/documents/{document_type}/generator", Name = "GenerateDocument")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateDocumentToCollaboratorAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, 
            [FromRoute] DocumentType document_type,
            [FromRoute] string identification_number
        )   
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var document = await _mediator.Send(new GenerateDocumentToCollaboratorQuery()
            {
                CompanyId = companie_id,
                DocumentType = document_type,
                IdentificationNumber = identification_number,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return File(document, "application/pdf", $"Documento_{identification_number}.pdf");;
        }
    }
}