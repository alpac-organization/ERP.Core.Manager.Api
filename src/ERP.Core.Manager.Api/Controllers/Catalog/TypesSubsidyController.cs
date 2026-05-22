using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.TypesSubsidy.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class TypesSubsidyController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Catalogos")] 
        [HttpGet("companies/{companie_id}/types-subsidy")]      
        [ProducesResponseType(typeof(List<TypeSubsidyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<TypeSubsidyDto>> GetTypesSubsidyAvailableAsync([FromRoute] Guid companie_id)
        {
            return await _mediator.Send(
                new GetTypesSubsidyAvailableQuery()
                {
                    CompanyId = companie_id
                }
            );
        }

    }
}
