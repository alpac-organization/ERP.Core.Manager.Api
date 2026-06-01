using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Companies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.CostCenters.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CostCenterController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Centros de costo")]  
        [HttpPost("companies/{company_id}/areas/{area_id}/cost-centers")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterCostCenterAsync([FromRoute] Guid company_id, [FromRoute] Guid area_id, [FromBody] RegisterCostCenterCommand payload)
        {
            payload.CompanyId = company_id;
            payload.AreaId = area_id;
            
            await _mediator.Send(payload);
            return Created();
        }

        // [Tags("Empresas")]  
        // [HttpGet("companies/{company_id}/areas/{area_id}/cost-centers")]   
        // [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status200OK)]
        // [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        // public async Task<CreatedResult> RegisterCostCenterAsync()
        // {
            

        //     return Created();
        // }
    }
}
