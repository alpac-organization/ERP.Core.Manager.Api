using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class ModulesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Modulos")] 
        [HttpGet("companies/{companie_id}/modules")]      
        [ProducesResponseType(typeof(List<ModuleDto>), StatusCodes.Status200OK)]  
        public async Task<List<ModuleDto>> ObtainActiveModulesByCompanyIdAsync([FromRoute] int companie_id)
        {
            return await _mediator.Send(
                new ObtainActiveModulesByCompanyIdQuery()
                {
                    CompanyId = companie_id
                }
            );
        }

        [HttpPost("companies/{companie_id}/modules")]      
        [Tags("Modulos")] 
        public async Task<IActionResult> CreateModuleAssociatedWithCompanyAsync(
            [FromRoute] int companie_id, 
            [FromBody] CreateModuleAssociatedWithCompanyCommand body
        ) {
            await _mediator.Send(
                new CreateModuleAssociatedWithCompanyCommand()
                {
                    CompanyId = companie_id,
                    ModuleName = body.ModuleName
                }
            );

            return Created(string.Empty, null);
        }
    }
}
