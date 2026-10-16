

using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class SanctionsController(IMediator _mediator) : ApiControllerBase
   {

   }
}