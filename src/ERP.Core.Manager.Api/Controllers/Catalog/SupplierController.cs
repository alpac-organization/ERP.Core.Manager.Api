using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Catalogs.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class SuppliersController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Proveedores")]
        [HttpGet("companies/{companie_id}/modules/{module_code}/suppliers")]
        [ProducesResponseType(typeof(PagedResponse<SupplierDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PagedResponse<SupplierDto>> GetSuppliersAsync([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromQuery] int page_size = 10,
            [FromQuery] int page_number = 1
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetSuppliersQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                PageSize = page_size,
                PageNumber = page_number,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        [Tags("Proveedores")]
        [HttpPost("companies/{companie_id}/modules/{module_code}/suppliers")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterSupplierAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromBody] RegisterSupplierCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companie_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(payload);

            return Created();
        }


        [Tags("Proveedores")]
        [HttpPatch("companies/{companie_id}/modules/{module_code}/suppliers/{supplier_id}")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> UpdaterSupplierInformationAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] Guid supplier_id, [FromBody] UpdateSupplierInformationCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companie_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(payload);

            return Ok();
        }

    }
}
