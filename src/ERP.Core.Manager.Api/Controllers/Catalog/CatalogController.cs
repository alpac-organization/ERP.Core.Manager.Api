using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CatalogsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Catologos")]
        [HttpGet("companies/{companie_id}/catalogs/{catalog_type}/details")]
        [ProducesResponseType(typeof(List<CatalogDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<CatalogDetailsDto>> GetCatalogsDetailsByCatalogIdAsync([FromRoute] Guid companie_id, [FromRoute] CatalogType catalog_type)
        {
            var result = await _mediator.Send(new GetCatalogsDetailsByCatalogIdQuery()
            {
                CatalogType = catalog_type,
                CompanyId = companie_id
            });

            return result;
        }

    }
}