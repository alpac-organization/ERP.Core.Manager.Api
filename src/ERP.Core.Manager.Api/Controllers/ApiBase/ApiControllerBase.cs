using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.ApiBase
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        private IMediator? _mediator;

        protected IMediator Mediator => 
            _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

        protected Guid ResolveUserId()
        {
            var userId = HttpContext.Items["UserId"] as string;
            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : Guid.Empty;
        }
    }
}