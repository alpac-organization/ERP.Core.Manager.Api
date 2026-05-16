using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class DeductionsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Deducciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/deductions")]      
        [ProducesResponseType(typeof(PagedResponseDeduction<DeductionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponseDeduction<DeductionDto>> GetDeductionsHistoryAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] string module_code,
            [FromQuery] string? identification_number,
            [FromQuery] DeductionType? type,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var deductionHistory = await _mediator.Send(new GetDeductionsHistoryQuery()
            {
                CompanyId = companie_id,
                DeductionType = type,
                IdentificationNumber = identification_number,
                ModuleCode = module_code,
                PageNumber = page_number,
                PageSize = page_size,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return deductionHistory;            
        }

        [Tags("Deducciones")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/deductions")]
        [ProducesResponseType(typeof(CreatedResult ), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterDeductionAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromBody] RegisterDeductionCommand Payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            Payload.CompanyId = companie_id;
            Payload.ModuleCode = module_code;
            Payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(Payload);
            
            return Created();         
        }
    }
}