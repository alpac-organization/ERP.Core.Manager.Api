using MediatR;
using Microsoft.AspNetCore.Mvc;

using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;

using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Controllers.Catalog;

[HasToken]
[ApiVersion("1.0")]
[Route("api/v1/")]
[Tags("Conceptos de Salida de Inventario")]
public class ReasonsInventoryRemovalController(IMediator _mediator) : ApiControllerBase
{
    [HttpGet("companies/{company_id}/modules/{module_code}/inventory/reasons-removals")]
    [ProducesResponseType(typeof(PagedResponse<ReasonsInventoryRemovalDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<PagedResponse<ReasonsInventoryRemovalDto>> GetReasonsInventoryRemovalAsync([FromRoute] Guid company_id, [FromRoute] string module_code,
        [FromQuery] int page_number = 1,
        [FromQuery] int page_size = 10
    )
    {
        var userIdStr = HttpContext.Items["UserId"] as string;

        return await _mediator.Send(new GetReasonsInventoryRemovalQuery
        {
            CompanyId = company_id,
            ModuleCode = module_code,
            PageNumber = page_number,
            PageSize = page_size,
            UserId = Guid.Parse(userIdStr ?? "")
        });
    }

    [HttpPost("companies/{company_id}/modules/{module_code}/inventory/reasons-removals")]
    [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<CreatedResult> RegisterReasonsInventoryRemovalAsync(
        [FromRoute] Guid company_id,
        [FromRoute] string module_code,
        [FromBody] RegisterReasonsInventoryRemovalCommand payload,
        CancellationToken cancellationToken)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        payload.CompanyId = company_id;
        payload.ModuleCode = module_code;
        payload.UserId = Guid.Parse(userIdStr ?? "");

        await _mediator.Send(payload, cancellationToken);

        return Created();
    }
}