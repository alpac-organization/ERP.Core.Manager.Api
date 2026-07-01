using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Subsidies.v1.Dtos;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class SubsidiesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Subsidio")]
        [HttpGet("companies/{companie_id}/modules/{module_code}/subsidies")]
        [ProducesResponseType(typeof(PagedResponse<SubsidyHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PagedResponse<SubsidyHistoryDto>> GetSubsidiesHistoryAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] string module_code,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10,
            [FromQuery] string? identification_number = null,
            [FromQuery] Guid? area_id = null,
            [FromQuery] Guid? branch_id = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetSubsidiesHistoryQuery
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                IdentificationNumber = identification_number,
                AreaId = area_id,
                BranchId = branch_id ?? Guid.Empty,
                PageNumber = page_number,
                PageSize = page_size
            });
        }

        [Tags("Subsidio")]
        [HttpPost("companies/{companie_id}/modules/{module_code}/collaborators/{collaborator_id}/subsidies")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterSubsidyAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] string module_code,
            [FromRoute] Guid collaborator_id,
            [FromBody] RegisterSubsidyCommmand payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companie_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.CollaboratorId = collaborator_id;

            await _mediator.Send(payload);

            return Ok();
        }
    }
}