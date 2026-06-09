using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class JobPositionsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Centros de costo")]  
        [HttpPost("companies/{company_id}/job-positions")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterJobPositionAsync([FromRoute] Guid company_id)
        {

            return Created();
        }

        [Tags("Centros de costo")]  
        [HttpDelete("companies/{company_id}/job-positions/{job_position_id}")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<NoContentResult> DeleteCostCenterAsync([FromRoute] Guid company_id, [FromRoute] Guid job_position_id)
        {


            return NoContent();
        }

        [Tags("Centros de costo")]  
        [HttpGet("companies/{company_id}/job-positions")]   
        [ProducesResponseType(typeof(List<CostCenterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<CostCenterDto>> GetCostCenterByAreaAsync([FromRoute] Guid company_id, [FromRoute] Guid area_id)
        {
            return await _mediator.Send(new GetCostCentersByAreaQuery()
            {
                CompanyId = company_id,
                AreaId = area_id
            });
        }
    }
}
