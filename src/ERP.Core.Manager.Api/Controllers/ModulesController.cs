using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Modules.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class ModulesController(IMediator _mediator) : ApiControllerBase
    {
        [HttpGet("companies/{companie_id}/modules")]      
        [Tags("Modulos")] 
        public async Task<List<ModuleDto>> ObtainActiveModulesByCompanyIdAsync([FromRoute] int companie_id)
        {
            return await _mediator.Send(
                new ObtainActiveModulesByCompanyIdQuery()
                {
                    CompanyId = companie_id
                }
            );
        }
    }
}
