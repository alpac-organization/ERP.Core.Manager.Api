using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Billing.Api.Controllers.ApiBase;

namespace ERP.Core.Billing.Api.Controllers
{
    [Route("api/v1")]
    [ApiVersion("1.0")]
    public class TodoController(IMediator _mediator) : ApiControllerBase
    {
        [HttpPost("/companies/{companie_id}/auth/login")]       
        public async Task StartLoginWithUserAsync([FromBody] Object command)
        {
            var todoId = await _mediator.Send(command);
        }

        [HttpPost("/companies/{companie_id}/auth/logout")]       
        public async Task StartLogoutOfUserAsync([FromBody] Object command)
        {
            var todoId = await _mediator.Send(command);
        }
    }
}
