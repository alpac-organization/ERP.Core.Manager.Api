using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class WorkAreasController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Areas de trabajo")]  
        [HttpPost("companies/{company_id}/modules/{module_code}/areas")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterWorkAreaAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromBody] RegisterWorkAreaCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            
            await _mediator.Send(payload);

            return Created();
        }

        [Tags("Centros de costo")]  
        [HttpGet("companies/{company_id}/modules/{module_code}/areas")]   
        [ProducesResponseType(typeof(List<WorkAreaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<WorkAreaDto>> GetWorkAreasAsync([FromRoute] Guid company_id, [FromRoute] string module_code)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetWorkAreasQuery()
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        [Tags("Areas de trabajo")]  
        [HttpDelete("companies/{company_id}/modules/{module_code}/areas/{area_id}")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<NoContentResult> DeleteWorkAreaAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromRoute] Guid area_id)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var payload = new DeleteWorkAreaCommand()
            {
                WorkAreaId = area_id,
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            };

            await _mediator.Send(payload);

            return NoContent();
        }
    }
}
