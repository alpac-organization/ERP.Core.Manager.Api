using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;
using Microsoft.AspNetCore.Authorization;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Errors;

namespace ERP.Core.Manager.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class AuthenticationController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Autenticación")]   
        [HttpPost("companies/{companie_id}/auth/login")] 
        [ProducesResponseType(typeof(LoginDto),      StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        public async Task<LoginDto> LoginWithUsernameOrEmailWithPasswordAsync([FromRoute] int companie_id, [FromBody] LoginWithUsernameAndPasswordCommand payload)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            return await _mediator.Send(
                new LoginWithUsernameAndPasswordCommand()
                {
                    CompanyId = companie_id,
                    Email = payload.Email,
                    Password = payload.Password,
                    Username = payload.Username,
                    SessionDetails = new()
                    {
                        DeviceName = payload.SessionDetails?.DeviceName,
                        IpAddress = remoteIp
                    }
                }
            );
        }

        [Authorize]
        [Tags("Autenticación")]
        [HttpPost("companies/{companie_id}/auth/refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync([FromRoute] int companie_id)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();


            return Created(string.Empty, null);
        }


        [Authorize]
        [Tags("Autenticación")]
        [HttpPost("companies/{companie_id}/auth/logout")]
        public async Task<IActionResult> LogoutAsync([FromRoute] int companie_id)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();


            return Created(string.Empty, null);
        }

    }
}
