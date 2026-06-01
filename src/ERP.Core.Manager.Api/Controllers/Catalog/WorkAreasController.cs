using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.WorkAreas.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class WorkAreasController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Areas de trabajo")]  
        [HttpPost("companies/{company_id}/areas")]   
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterWorkAreaAsync([FromRoute] Guid company_id, [FromBody] RegisterWorkAreaCommand payload)
        {
            payload.CompanyId = company_id;
            
            await _mediator.Send(payload);

            return Created();
        }

        [Tags("Centros de costo")]  
        [HttpGet("companies/{company_id}/areas")]   
        [ProducesResponseType(typeof(List<WorkAreaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<WorkAreaDto>> GetWorkAreasAsync([FromRoute] Guid company_id)
        {
            return await _mediator.Send(new GetWorkAreasQuery()
            {
                CompanyId = company_id,
            });
        }
    }
}
