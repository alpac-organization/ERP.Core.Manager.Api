using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Notifications
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class NotificationsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Notificaciones Push")]
        [HttpPost("push-tokens")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterPushTokenAsync([FromBody] RegisterPushTokenCommand payload)
        {
            await _mediator.Send(payload);

            return Created();
        }

        [Tags("Notificaciones Push")]
        [HttpPost("push-tokens/unlink")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> UnlinkPushTokenAsync([FromBody] UnlinkPushTokenCommand payload)
        {
            await _mediator.Send(payload);

            return Ok();
        }
    }
}