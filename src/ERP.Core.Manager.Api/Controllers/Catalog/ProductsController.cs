using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
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
public class ProductsController(IMediator _mediator) : ApiControllerBase
{
    [Tags("Productos")]
    [HttpGet("companies/{companie_id}/modules/{module_code}/products", Name = "GetProducts")]
    [ProducesResponseType(typeof(PagedResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<PagedResponse<ProductDto>> GetProductsAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromQuery] Guid? category_product_id,
        [FromQuery] int page_number = 1,
        [FromQuery] int page_size = 10)
    {
        var userId = ResolveUserId();

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
    [HttpGet("companies/{companie_id}/modules/{module_code}/products/{product_id}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ProductDetailDto> GetProductDetailsAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromRoute] Guid product_id,
        [FromQuery] int page_size = 10,
        [FromQuery] int page_number = 1)
    {
        var userId = ResolveUserId();

        return await _mediator.Send(new GetProductDetailsQuery
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            ProductId = product_id,
            PageSize = page_size,
            PageNumber = page_number
        });
    }

    [Tags("Productos")]
    [HttpPost("companies/{companie_id}/modules/{module_code}/products")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterProductAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromBody] RegisterProductCommand command)
    {
        var userId = ResolveUserId();

        command.UserId = userId;
        command.CompanyId = companie_id;
        command.ModuleCode = module_code;

        var productId = await _mediator.Send(command);
        return Ok(new { productId });
    }

    [Tags("Productos")]
    [HttpPatch("companies/{companie_id}/modules/{module_code}/products/{product_id}")]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkResult> UpdateProductAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromRoute] Guid product_id,
        [FromBody] UpdateProductCommand payload)
    {
        var userId = ResolveUserId();

        payload.UserId = userId;
        payload.CompanyId = companie_id;
        payload.ModuleCode = module_code;
        payload.ProductId = product_id;

        await _mediator.Send(payload);
        return Ok();
    }

    [Tags("Productos")]
    [HttpPatch("companies/{companie_id}/modules/{module_code}/products/{product_id}/suppliers/{supplier_id}/prices")]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkResult> UpdateSupplierProductPriceAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromRoute] Guid product_id,
        [FromRoute] Guid supplier_id,
        [FromBody] UpdateSupplierProductPriceCommand payload)
    {
        var userId = ResolveUserId();

        payload.UserId = userId;
        payload.CompanyId = companie_id;
        payload.ModuleCode = module_code;
        payload.ProductId = product_id;
        payload.SupplierId = supplier_id;

        await _mediator.Send(payload);
        return Ok();
    }

    [Tags("Productos")]
    [HttpGet("companies/{companie_id}/modules/{module_code}/products/{product_id}/suppliers/{supplier_id}/price-history")]
    [ProducesResponseType(typeof(PagedResponse<SupplierProductPriceHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<PagedResponse<SupplierProductPriceHistoryDto>> GetSupplierProductPriceHistoryAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromRoute] Guid product_id,
        [FromRoute] Guid supplier_id,
        [FromQuery] int page_size = 10,
        [FromQuery] int page_number = 1,
        [FromQuery] SupplierPriceHistoryType? price_type = null)
    {
        var userId = ResolveUserId();

        return await _mediator.Send(new GetSupplierProductPriceHistoryQuery
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            ProductId = product_id,
            SupplierId = supplier_id,
            PageSize = page_size,
            PageNumber = page_number,
            PriceType = price_type
        });
    }

    [Tags("Productos")]
    [HttpDelete("companies/{companie_id}/modules/{module_code}/products/{product_id}/suppliers/{supplier_id}")]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkResult> DeactivateSupplierProductAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromRoute] Guid product_id,
        [FromRoute] Guid supplier_id)
    {
        var userId = ResolveUserId();

        await _mediator.Send(new DeactivateSupplierProductCommand
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            ProductId = product_id,
            SupplierId = supplier_id
        });

        return Ok();
    }

    private Guid ResolveUserId()
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        return Guid.TryParse(userIdStr, out var parseGuid) ? parseGuid : Guid.Empty;
    }
}
