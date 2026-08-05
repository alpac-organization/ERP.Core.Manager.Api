using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Users.v1.Commands;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Auth
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class UsersController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Usuarios")] 
        [HttpGet("companies/{companie_id}/users/modules")]    
        [ProducesResponseType(typeof(List<UserModuleDto>), StatusCodes.Status200OK)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<UserModuleDto>> GetAvailableModulesForUserAsync([FromRoute] Guid companie_id)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetAvailableModulesForUserQuery()
            {
                CompanyId = companie_id,
                UserId =  Guid.Parse(userIdStr ?? "")
            });

            return result;
        }

        
        [Tags("Usuarios")] 
        [HttpPost("companies/{companie_id}/users", Name = "CreateUser")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)] 
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)] 
        public async Task<CreateUserDto> CreateNewUserAsync([FromRoute] Guid companie_id, [FromBody] CreateNewUserCommand payload)
        {
            var command = new CreateNewUserCommand()
            {
                CompanyId = companie_id,
                FullName = payload.FullName,
                Email = payload.Email,
                Password = payload.Password,
                IdentificationNumber = payload.IdentificationNumber,
                UserType = payload.UserType,
                BranchId = payload.BranchId,
                AreaId = payload.AreaId,
                ModulesWithAccess = payload.ModulesWithAccess
            };

            var result = await _mediator.Send(command);

            return result;
        }
    }
}
