using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Sanctions.v1.Queries;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class SanctionsController(IMediator _mediator) : ApiControllerBase
   {
      [Tags("Sanciones Disciplinarias")]
      [HttpGet("companies/{company_id}/modules/{module_code}/sanctions")]
      [ProducesResponseType(typeof(PagedResponse<SanctionDto>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<PagedResponse<SanctionDto>> GetSanctionsAsync(
         [FromRoute] Guid company_id,
         [FromRoute] string module_code,
         [FromQuery] Guid? collaborator_id,
         [FromQuery] Guid? fault_id,
         [FromQuery] SanctionType? sanction_type,
         [FromQuery] SanctionStatus? sanction_status,
         [FromQuery] int page_size = 10,
         [FromQuery] int page_number = 1
      )
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         return await _mediator.Send(new GetSanctionsQuery
         {
            UserId = Guid.Parse(userIdStr ?? ""),
            CompanyId = company_id,
            ModuleCode = module_code,
            CollaboratorId = collaborator_id,
            FaultId = fault_id,
            SanctionType = sanction_type,
            SanctionStatus = sanction_status,
            PageSize = page_size,
            PageNumber = page_number
         });
      }

      [Tags("Sanciones Disciplinarias")]
      [HttpGet("companies/{company_id}/modules/{module_code}/sanctions/{sanction_id}")]
      [ProducesResponseType(typeof(SanctionDetailDto), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<SanctionDetailDto> GetSanctionDetailsAsync(
         [FromRoute] Guid company_id,
         [FromRoute] string module_code,
         [FromRoute] Guid sanction_id
      )
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         return await _mediator.Send(new GetSanctionDetailsQuery
         {
            UserId = Guid.Parse(userIdStr ?? ""),
            CompanyId = company_id,
            ModuleCode = module_code,
            SanctionId = sanction_id
         });
      }

      [Tags("Registro Sancion Disciplinaria")]
      [HttpPost("companies/{company_id}/modules/{module_code}/sanctions")]
      [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<CreatedResult> RegisterSanction(
         [FromRoute] Guid company_id,
         [FromRoute] string module_code,
         [FromBody] RegisterSanctionCommand payload
      )
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         payload.UserId = Guid.Parse(userIdStr ?? "");
         payload.CompanyId = company_id;
         payload.ModuleCode = module_code;

         await _mediator.Send(payload);

         return Created();
      }
   }
}
