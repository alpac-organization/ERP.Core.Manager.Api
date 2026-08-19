using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Commons.Mappings;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Customers.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog;

[HasToken]
[ApiVersion("1.0")]
[Route("api/v1/")]
public class CustomerController(IMediator _mediator) : ApiControllerBase
{

    [Tags("Clientes")]
    [HttpGet("companies/{company_id}/modules/{module_code}/customers")]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<CustomerDto>> GetCustomersAsync(
        [FromRoute] Guid company_id,
        [FromRoute] string module_code,
        [FromQuery] bool? status,
        [FromQuery] Guid? customer_type_id,
        CancellationToken cancellationToken)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid.TryParse(userIdStr, out var userId);

        return await _mediator.Send(new GetCustomersAvailableQuery
        {
            CompanyId = company_id,
            ModuleCode = module_code,
            UserId = userId,
            Status = status,
            CustomerTypeId = customer_type_id
        }, cancellationToken);
    }

    [Tags("Clientes")]
    [HttpGet("companies/{company_id}/customers/{customer_id}/details")]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkResult> GetCustomerDetailsAsync([FromRoute] Guid company_id, [FromRoute] Guid customer_id)
    {
        /*Pendiente a realizar*/

        return Ok();
    }


    [Tags("Clientes")]
    [HttpPost("companies/{company_id}/modules/{module_code}/customers")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkObjectResult> RegisterCustomerAsync(
        [FromRoute] Guid company_id,
        [FromRoute] string module_code,
        [FromBody] RegisterCustomerDto dto,
        CancellationToken cancellationToken)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid.TryParse(userIdStr, out var userId);

        var command = dto.ToCommand(
            userId: userId,
            companyId: company_id,
            moduleCode: module_code
        );

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(response);
    }

    [Tags("Clientes")]
    [HttpGet("companies/{company_id}/modules/{module_code}/customer-types")]
    [ProducesResponseType(typeof(List<CustomerTypeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<List<CustomerTypeDto>> GetCustomerTypesAsync(
        [FromRoute] Guid company_id,
        [FromRoute] string module_code,
        [FromQuery] bool? status,
        CancellationToken cancellationToken)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid.TryParse(userIdStr, out var userId);

        return await _mediator.Send(new GetCustomerTypesQuery
        {
            CompanyId = company_id,
            ModuleCode = module_code,
            UserId = userId,
            Status = status
        }, cancellationToken);
    }

    [Tags("Clientes")]
    [HttpPost("companies/{company_id}/modules/{module_code}/customer-types")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<OkObjectResult> RegisterCustomerTypeAsync(
        [FromRoute] Guid company_id,
        [FromRoute] string module_code,
        [FromBody] RegisterCustomerTypeDto dto,
        CancellationToken cancellationToken)
    {
        var userIdStr = HttpContext.Items["UserId"] as string;
        Guid.TryParse(userIdStr, out var userId);

        var command = dto.ToCommand(
            userId: userId,
            companyId: company_id,
            moduleCode: module_code
        );

        var response = await _mediator.Send(command, cancellationToken);

        return Ok(response);
    }
}