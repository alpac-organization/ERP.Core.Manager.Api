using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Terminations.v1.Queries;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class TerminationController(IMediator _mediator) : ApiControllerBase
   {
      [Tags("Baja de Colaboradores")]
      [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators-termination")]
      [ProducesResponseType(typeof(PagedResponse<TerminationDto>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<PagedResponse<TerminationDto>> GetTerminationsAsync(
         [FromRoute] Guid companie_id,
         [FromRoute] string module_code,
         [FromQuery] string? identification_number = null,
         [FromQuery] TerminationType? termination_type = null,
         [FromQuery] TerminationProcessStatus? termination_status = null,
         [FromQuery] int page_size = 10,
         [FromQuery] int page_number = 1)
      {
         return await _mediator.Send(new GetTerminationsQuery
         {
            UserId = ResolveUserId(),
            CompanyId = companie_id,
            ModuleCode = module_code,
            IdentificationNumber = identification_number,
            TerminationType = termination_type,
            TerminationStatus = termination_status,
            PageSize = page_size,
            PageNumber = page_number
         });
      }

      [Tags("Baja de Colaboradores")]
      [HttpGet("companies/{companie_id}/modules/{module_code}/collaborators-termination/{termination_id}")]
      [ProducesResponseType(typeof(TerminationDetailDto), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<TerminationDetailDto> GetTerminationDetailsAsync(
         [FromRoute] Guid companie_id,
         [FromRoute] string module_code,
         [FromRoute] Guid termination_id)
      {
         return await _mediator.Send(new GetTerminationDetailsQuery
         {
            UserId = ResolveUserId(),
            CompanyId = companie_id,
            ModuleCode = module_code,
            TerminationId = termination_id
         });
      }

      [Tags("Baja de Colaboradores")]
      [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators-termination")]
      [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<CreatedResult> RegisterTerminationAsync(
         [FromRoute] Guid companie_id,
         [FromRoute] string module_code,
         [FromBody] RegisterTerminationCommand payload)
      {
         payload.UserId = ResolveUserId();
         payload.CompanyId = companie_id;
         payload.ModuleCode = module_code;

         await _mediator.Send(payload);

         return Created();
      }

      [Tags("Baja de Colaboradores")]
      [HttpPatch("companies/{companie_id}/modules/{module_code}/collaborators-termination/{termination_id}/close")]
      [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<OkResult> CloseTerminationAsync(
         [FromRoute] Guid companie_id,
         [FromRoute] string module_code,
         [FromRoute] Guid termination_id)
      {
         await _mediator.Send(new CloseTerminationCommand
         {
            UserId = ResolveUserId(),
            CompanyId = companie_id,
            ModuleCode = module_code,
            TerminationId = termination_id
         });

         return Ok();
      }

      [Tags("Baja de Colaboradores")]
      [HttpPatch("companies/{companie_id}/modules/{module_code}/collaborators-termination/{termination_id}/cancel")]
      [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<OkResult> CancelTerminationAsync(
         [FromRoute] Guid companie_id,
         [FromRoute] string module_code,
         [FromRoute] Guid termination_id,
         [FromBody] CancelTerminationCommand payload)
      {
         payload.UserId = ResolveUserId();
         payload.CompanyId = companie_id;
         payload.ModuleCode = module_code;
         payload.TerminationId = termination_id;

         await _mediator.Send(payload);

         return Ok();
      }
   }
}
