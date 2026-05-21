using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesIncome.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class TypesIncomeController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Catalogos")] 
        [HttpGet("companies/{companie_id}/types-income")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<TypesIncomeDto>> GetTypesIncomeAvailableAsync([FromRoute] Guid companie_id)
        {
            return await _mediator.Send(
                new GetTypesIncomeAvailableQuery()
                {
                    CompanyId = companie_id
                }
            );
        }

    }
}
