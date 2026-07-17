using MediatR;
using Microsoft.AspNetCore.Mvc;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;

using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class UnitsMeasurementController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Unidades De Medida")]
        [HttpGet("companies/{companie_id}/module/{module_code}/units-measurement")]
        [ProducesResponseType(typeof(List<UnitMeasureDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<UnitMeasureDto>> GetCatalogsDetailsByCatalogIdAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, 
            [FromQuery] UnitMeasureType? unit_measure_type
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetUnitsMeasurementQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                UnitMeasureType = unit_measure_type
            });

            return result;
        }

    }
}