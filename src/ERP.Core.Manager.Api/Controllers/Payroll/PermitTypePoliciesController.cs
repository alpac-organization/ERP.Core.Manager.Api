using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.PermitTypePolicy.v1.commands;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class PermitTypePoliciesController(IMediator mediator) : ApiControllerBase
    {
        [Tags("Permisos")]
        [HttpGet("companies/{company_id}/modules/{module_code}/permit-form-options")]
        [ProducesResponseType(typeof(PermitFormOptionsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PermitFormOptionsDto> GetPermitFormOptionsAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await mediator.Send(new GetPermitFormOptionsQuery
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        [Tags("Permisos")]
        [HttpGet("companies/{company_id}/modules/{module_code}/permit-type-policies")]
        [ProducesResponseType(typeof(PagedResponse<PermitTypePolicyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PagedResponse<PermitTypePolicyDto>> GetPermitTypePoliciesAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromQuery] bool only_active = true,
            [FromQuery] PermitApplicationType? permit_application_type = null,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await mediator.Send(new GetPermitTypePoliciesQuery
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                OnlyActive = only_active,
                PermitApplicationType = permit_application_type,
                PageNumber = page_number,
                PageSize = page_size
            });
        }

        [Tags("Permisos")]
        [HttpPost("companies/{company_id}/modules/{module_code}/permit-type-policies")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterPermitTypePolicyAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromBody] RegisterPermissionTypePolicyCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await mediator.Send(payload);

            return Created();
        }

        [Tags("Permisos")]
        [HttpPatch("companies/{company_id}/modules/{module_code}/permit-type-policies/{policy_id}")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> UpdatePermitTypePolicyAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromRoute] Guid policy_id,
            [FromBody] UpdatePermitTypePolicyCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.PermitTypePolicyId = policy_id;

            await mediator.Send(payload);

            return Ok();
        }
    }
}
