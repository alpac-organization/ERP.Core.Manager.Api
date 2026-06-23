using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Commands;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class DeductionsController(IMediator _mediator) : ApiControllerBase
    {
        
        //✅Registrar una nueva deducción
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


        //✅Obtener deducciones registradas
        [Tags("Deducciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/deductions")]      
        [ProducesResponseType(typeof(PagedResponseDeduction<DeductionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponseDeduction<DeductionDto>> GetDeductionsByAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] string module_code,

            [FromQuery] DeductionType? type,
            [FromQuery] DeductionStatus? status,
            [FromQuery] string? identification_number,

            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var deductionHistory = await _mediator.Send(new GetDeductionsActiveQuery()
            {
                DeductionType = type,
                DeductionStatus = status,

                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),

                IdentificationNumber = identification_number,
                PageSize = page_size,
                PageNumber = page_number
            });

            return deductionHistory;            
        }

        //✅Obtener detalles de la deducción
        [Tags("Deducciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/deductions/{deduction_id}/details")]      
        [ProducesResponseType(typeof(PagedResponseDeduction<DeductionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<DeductionDetailsDto> GetDeductionsDetailsAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] Guid deduction_id,
            [FromRoute] string module_code,
            [FromQuery] string? identification_number 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var payload = new GetDeductionDetailsQuery()
            {
                CompanyId = companie_id,
                DeductionId = deduction_id,
                UserId = Guid.Parse(userIdStr ?? ""),
                ModuleCode = module_code,
                IdentificationNumber = identification_number
            };

            return await _mediator.Send(payload);
        }


        //✅Obtener detalles de pagos realizados a la deducción
        [Tags("Deducciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/deductions/{deduction_id}/payments")]      
        [ProducesResponseType(typeof(PagedResponseDeduction<DeductionPaymentsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponseDeduction<DeductionPaymentsDto>> GetDeductionsPaymentsAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] Guid deduction_id,
            [FromRoute] string module_code,

            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetDeductionPaymentsQuery()
            {
                CompanyId = companie_id,
                DeductionId = deduction_id,
                ModuleCode = module_code,
                PageNumber = page_number,
                PageSize = page_size,
                UserId = Guid.Parse(userIdStr ?? "") 
            });
        }
    }
}