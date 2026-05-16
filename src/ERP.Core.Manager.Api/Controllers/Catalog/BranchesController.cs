using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Branches.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class BranchesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Catologos")]
        [HttpGet("companies/{companie_id}/branches")]
        [ProducesResponseType(typeof(List<BranchesDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<BranchesDto>> GetBranchesAvailableAsync([FromRoute] Guid companie_id)
        {
            var result = await _mediator.Send(new GetBranchesAvailableQuery()
            {
                CompanyId = companie_id
            });

            return result;
        }

    }
}