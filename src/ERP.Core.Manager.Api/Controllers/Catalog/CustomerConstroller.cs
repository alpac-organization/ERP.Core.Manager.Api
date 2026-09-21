using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CustomerController(IMediator _mediator) : ApiControllerBase
    {


        [Tags("Clientes")]
        [HttpPost("companies/{company_id}/modules/{module_code}/customers")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterCustomerAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code
            // Crear tu command de forma correcta
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return Created();
        }

        [Tags("Clientes")]
        [HttpGet("companies/{company_id}/modules/{module_code}/customers")]
        [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<CustomerDto>> GetCustomersAsync(
            [FromRoute] Guid company_id,
            [FromQuery] bool? status,
            [FromRoute] string module_code
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetCustomersAvailableQuery
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                Status = status,
            });
        }

        [Tags("Clientes")]
        [HttpGet("companies/{company_id}/modules/{module_code}/customers/{customer_id}/details")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> GetCustomerDetailsAsync([FromRoute] Guid company_id, [FromRoute] string module_code, [FromRoute] Guid customer_id)
        {
            /*Pendiente a realizar la logica del detalle del cliente.*/

            return Ok();
        }
    }
}
