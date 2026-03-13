using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Billing.Api.Controllers.ApiBase
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator => 
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
    }
}