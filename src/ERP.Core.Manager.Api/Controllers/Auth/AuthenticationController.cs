using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Authentication.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Errors;

namespace ERP.Core.Manager.Api.Controllers.Auth
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
            var deviceName = Request.Headers["x-device-name"].ToString();

            return await _mediator.Send(
                new LoginWithUsernameAndPasswordCommand()
                {
                    CompanyId = companie_id,
                    Email = payload.Email,
                    Password = payload.Password,
                    Username = payload.Username,
                    SessionDetails = new()
                    {
                        DeviceName = deviceName,
                        IpAddress = remoteIp
                    }
                }
            );
        }

        [Tags("Autenticación")]
        [HttpPost("companies/{companie_id}/auth/refresh-token")]
        [ProducesResponseType(typeof(LoginDto),      StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<LoginDto> RefreshTokenAsync([FromRoute] int companie_id, [FromBody] RefreshTokenCommand body)
        {
            return await _mediator.Send(new RefreshTokenCommand()
            {
                CompanyId = companie_id,
                RefreshToken = body.RefreshToken
            });
        }


        [Tags("Autenticación")]
        [HttpPost("companies/{companie_id}/auth/logout")]
        public async Task<IActionResult> LogoutAsync([FromRoute] int companie_id)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();


            return Created(string.Empty, null);
        }

    }
}
