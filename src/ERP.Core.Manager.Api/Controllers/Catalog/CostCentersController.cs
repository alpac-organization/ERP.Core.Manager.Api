using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CostCenterController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Centros de costo")]  
        [HttpPost("companies/{company_id}/modules/{module_code}/areas/{area_id}/cost-centers")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterCostCenterAsync([FromRoute] Guid company_id,[FromRoute] string module_code, [FromRoute] Guid area_id, [FromBody] RegisterCostCenterCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.AreaId = area_id;
            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(payload);
            return Created();
        }

        [Tags("Centros de costo")]  
        [HttpGet("companies/{company_id}/modules/{module_code}/areas/{area_id}/cost-centers")]   
        [ProducesResponseType(typeof(List<CostCenterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<CostCenterDto>> GetCostCenterByAreaAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromRoute] Guid area_id)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetCostCentersByAreaQuery()
            {
                AreaId = area_id,
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
            });
        }

        [Tags("Centros de costo")]  
        [HttpDelete("companies/{company_id}/modules/{module_code}/areas/{area_id}/cost-centers/{cost_center_id}")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<NoContentResult> DeleteCostCenterAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromRoute] Guid area_id, [FromRoute] Guid cost_center_id)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new DeleteCostCenterCommand()
            {
                AreaId = area_id,
                CompanyId = company_id,
                ModuleCode = module_code,
                CostCenterId = cost_center_id,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return NoContent();
        }
    }
}
