

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Faults.v1.Queries;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
   [HasToken]
   [ApiVersion("1.0")]
   [Route("api/v1/")]
   public class FaultsController(IMediator _mediator) : ApiControllerBase
   {
      [Tags("Faltas Disciplinarias")]
      [HttpGet("companies/{company_id}/modules/{module_code}/fault-catalogs")]
      [ProducesResponseType(typeof(PagedResponse<FaultCatalogDto>), StatusCodes.Status200OK)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
      [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
      public async Task<PagedResponse<FaultCatalogDto>> GetFaultCatalogsAsync(
         [FromRoute] Guid company_id,
         [FromRoute] string module_code,
         [FromQuery] FaultSeverity? severity,
         [FromQuery] string? code = "",
         [FromQuery] int page_size = 10,
         [FromQuery] int page_number = 1
      )
      {
         var userIdStr = HttpContext.Items["UserId"] as string;

         return await _mediator.Send(new GetFaultCatalogQuery
         {
            UserId = Guid.Parse(userIdStr ?? ""),
            CompanyId = company_id,
            ModuleCode = module_code,
            Code = code,
            FaultSeverity = severity,
            PageNumber = page_number,
            PageSize = page_size
         });
      }
   }
}