using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class UsersController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Usuarios")] 
        [HttpGet("companies/{companie_id}/users")]    
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<UserDto>> GetAllActiveUsersAsync([FromRoute] int companie_id)
        {
            var result = await _mediator.Send(new GetAllActiveUsersByCompanyIdQuery(companie_id));
            return result;
        }
        
        [Tags("Usuarios")] 
        [HttpPost("companies/{companie_id}/users", Name = "CreateUser")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)] 
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)] 
        public async Task<CreateUserDto> CreateNewUserAsync([FromRoute] int companie_id, [FromBody] CreateNewUserCommand payload)
        {
            var commands = new CreateNewUserCommand()
            {
                Username = payload.Username,
                CompanyId = companie_id,
                FullName = payload.FullName,
                Email = payload.Email,
                Password = payload.Password,
                ModulesWithAccess = payload.ModulesWithAccess
            };

            var result = await _mediator.Send(commands);

            return result;
        }

        [Tags("Usuarios")] 
        [HttpPost("companies/{companie_id}/users/{user_id}/roles", Name = "GetRolesAndPermissions")]
        public async Task<IActionResult> ObtainUserRolesAndPermissionsAsync([FromRoute] int companie_id, [FromRoute] int user_id) 
        {
           return Ok();
        }
    }
}
