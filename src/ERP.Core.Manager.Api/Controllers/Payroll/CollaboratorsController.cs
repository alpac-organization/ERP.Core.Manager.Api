using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CollaboratorsController(IMediator _mediator) : ApiControllerBase
    {
        [HttpGet("companies/{companie_id}/collaborators")]      
        [Tags("Colaboradores")] 
        public async Task<IActionResult> GetActiveContributors([FromRoute] int companie_id)
        {
            return Ok();
        }

        [HttpPost("companies/{companie_id}/collaborators/{collaborator_id}/details")]      
        [Tags("Colaboradores")] 
        public async Task<IActionResult> ObtenerRolesYPermisosDelUsuario([FromRoute] int companie_id, [FromRoute] int collaborator_id) 
        {
           return Ok();
        }
    }
}
