using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class UsersController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Usuarios")] 
        [HttpGet("companies/{companie_id}/users")]    
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]  
        public async Task<List<UserDto>> GetAllActiveUsersAsync([FromRoute] int companie_id)
        {
            var result = await _mediator.Send(new GetAllActiveUsersByCompanyIdQuery(companie_id));
            return result;
        }

        [Tags("Usuarios")] 
        [HttpPost("companies/{companie_id}/users/{user_id}/roles")]
        public async Task<IActionResult> ObtainUserRolesAndPermissionsAsync([FromRoute] int companie_id, [FromRoute] int user_id) 
        {
           return Ok();
        }
    }
}
