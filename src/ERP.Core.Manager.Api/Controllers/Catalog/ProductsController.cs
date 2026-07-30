using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
namespace ERP.Core.Manager.Api.Controllers.Catalog;

using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
[HasToken]
[ApiVersion("1.0")]
[Route("api/v1/")]
public class ProductsController(IMediator _mediator) : ApiControllerBase
{
    [Tags("Productos")]
    [HttpGet("companies/{companie_id}/modules/{module_code}/products", Name = "GetProducts")]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<PagedResponse<ProductDto>> GetProductsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromQuery] Guid? category_product_id, [FromQuery] int page_number = 1, [FromQuery] int page_size = 10)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid userId = Guid.TryParse(userIdStr, out var parseGuid) ? parseGuid : Guid.Empty;

        return await _mediator.Send(new GetProductsQuery
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            CategoryProductId = category_product_id,
            PageNumber = page_number,
            PageSize = page_size
        });
    }

    [Tags("Productos")]
    [HttpPost("companies/{companie_id}/modules/{module_code}/products")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterProductAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromBody] RegisterProductCommand command)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid userId = Guid.TryParse(userIdStr, out var parseGuid) ? parseGuid : Guid.Empty;

        command.UserId = userId;
        command.CompanyId = companie_id;
        command.ModuleCode = module_code;

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }
}