using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class UsersController(IMediator _mediator) : ApiControllerBase
    {
        [HttpGet("companies/{companie_id}/users")]      
        [Tags("Usuarios")] 
        public async Task<IActionResult> GetAllActiveUsersAsync([FromRoute] int companie_id)
        {
            return Ok();
        }

        [HttpPost("companies/{companie_id}/users/{user_id}/roles")]      
        [Tags("Modulos")] 
        public async Task<IActionResult> ObtainUserRolesAndPermissionsAsync([FromRoute] int companie_id, [FromRoute] int user_id) 
        {
           return Ok();
        }
    }
}
