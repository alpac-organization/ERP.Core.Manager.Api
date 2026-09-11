using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.JobPositions.v1.Commands;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class JobPositionsController(IMediator _mediator) : ApiControllerBase
   {
      [Tags("Catalogos")]
      [HttpPost("companies/{company_id}/modules/{module_code}/job-positions")]
      [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<CreatedResult> RegisterJobPositionAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromBody] RegisterJobPositionCommand payload)
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         payload.CompanyId = company_id;
         payload.ModuleCode = module_code;
         payload.UserId = Guid.Parse(userIdStr ?? "");

         await _mediator.Send(payload);
         
         return Created();
      }

      [Tags("Catalogos")]
      [HttpGet("companies/{company_id}/modules/{module_code}/job-positions")]
      [ProducesResponseType(typeof(List<JobPositionDto>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<List<JobPositionDto>> GetJobPositionsAsync([FromRoute] Guid company_id, [FromRoute] string module_code)
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         return await _mediator.Send(new GetJobPositionsQuery()
         {
            CompanyId = company_id,
            ModuleCode = module_code,
            UserId = Guid.Parse(userIdStr ?? "")
         });
      }

      [Tags("Catalogos")]
      [HttpDelete("companies/{company_id}/modules/{module_code}/job-positions/{job_position_id}")]
      [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<NoContentResult> DeleteJobPositionAsync([FromRoute] Guid company_id,[FromRoute] string module_code, [FromRoute] Guid job_position_id)
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         await _mediator.Send(new DeleteJobPositionCommand()
         {
            CompanyId = company_id,
            ModuleCode = module_code,
            JobPositionId = job_position_id,
            UserId = Guid.Parse(userIdStr ?? "")
         });

         return NoContent();
      }
   }
}
