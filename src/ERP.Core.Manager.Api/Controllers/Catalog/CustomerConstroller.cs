using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class CustomerController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Clientes")]  
        [HttpGet("companies/{company_id}/customers")]   
        [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<CustomerDto>> RegisterCustomerAsync([FromRoute] Guid company_id, 
            [FromQuery] bool? status = true, 
            [FromQuery] Guid? customer_type_id = null        
        )
        {
            return await _mediator.Send(new GetCustomersAvailableQuery()
            {
                Status = status,
                CompanyId = company_id,
                CustomerTypeId = customer_type_id,
            });
        }

        [Tags("Clientes")]  
        [HttpGet("companies/{company_id}/customers/{customer_id}/details")]   
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> GetCustomerDetailsAsync([FromRoute] Guid company_id,  [FromRoute] Guid customer_id)
        {
            /*Pendiente a realizar*/

            return Ok();
        }
    }
}
