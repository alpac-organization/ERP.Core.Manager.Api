

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using QRCoder.Extensions;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class SanctionsController(IMediator _mediator) : ApiControllerBase
   {
      [Tags("Catalogo Faltas Disciplinarias")]
      [HttpGet("companies/{company_id}/modules/{module_code}/sanctions")]
      [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<string> GetSanctionsAsync(
               [FromRoute] Guid company_id,
               [FromRoute] string module_code,
               [FromQuery] int page_size = 10,
               [FromQuery] int page_number = 1
            )
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         return "";
      }
   }
}