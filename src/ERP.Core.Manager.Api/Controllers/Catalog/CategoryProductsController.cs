using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.CategoryProducts.v1.Queries;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Core.Manager.Api.Controllers.Catalog;

[HasToken]
[ApiVersion("1.0")]
[Route("api/v1/")]
public class CategoryProductController(IMediator _mediator) : ApiControllerBase
{
    [Tags("Categorías de Productos")]
    [HttpGet("companies/{companie_id}/modules/{module_code}/category-products")]
    [ProducesResponseType(typeof(List<CategoryProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<CategoryProductDto>> GetCategoryProductsTreeAsync(
        [FromRoute] Guid companie_id,
        [FromRoute] string module_code,
        [FromQuery] Guid? parent_id)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;

        Guid userId = Guid.TryParse(userIdStr, out var parseGuid) ? parseGuid : Guid.Empty;

        return await _mediator.Send(new GetCategoryProductsTreeQuery
        {
            CompanyId = companie_id,
            ModuleCode = module_code,
            UserId = userId,
            ParentId = parent_id
        });
    }
}