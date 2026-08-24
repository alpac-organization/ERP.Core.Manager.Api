using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Notifications.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Notifications
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class NotificationsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Notificaciones")]
        [HttpPost("companies/{companyId}/notifications/register-device-token")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterPushTokenAsync([FromRoute] Guid companyId, [FromBody] RegisterPushTokenCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companyId;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(payload);

            return Created();
        }

        [Tags("Notificaciones")]
        [HttpPost("companies/{companyId}/notifications/unlink-arn-token")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> UnlinkPushTokenAsync([FromRoute] Guid companyId, [FromBody] UnlinkPushTokenCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
    
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.CompanyId = companyId;

            await _mediator.Send(payload);

            return Ok();
        }

        [Tags("Notificaciones")]
        [HttpGet("companies/{companyId}/notifications")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> GetNotificationsAsync([FromRoute] Guid companyId)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            //Your code here.


            return Ok();
        }
    }
}