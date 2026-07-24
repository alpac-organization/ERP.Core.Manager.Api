using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
namespace ERP.Core.Manager.Api.Controllers.Catalog;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

[HasToken]
[ApiVersion("1.0")]
[Route("api/v1/")]
public class ProductsController(IMediator _mediator) : ApiControllerBase
{
    [Tags("Productos")]
    [HttpGet("companies/{companie_id}/modules/{module_code}/products")]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]

    public async Task<List<ProductDto>> GetProductsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromQuery] Guid? product_id)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid userId = Guid.TryParse(userIdStr, out var parseGuid) ? parseGuid : Guid.Empty;

        return await _mediator.Send(new GetProductsQuery
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            ProductId = product_id
        });
    }
}