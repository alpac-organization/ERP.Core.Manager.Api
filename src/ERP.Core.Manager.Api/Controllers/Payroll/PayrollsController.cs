using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Commands;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class PayrollsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Nomina")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/payrolls-status")]      
        [ProducesResponseType(typeof(CheckPayrollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CheckPayrollDto> CheckIfThereIsPayrollInProgressAsync([FromRoute] Guid companie_id,  [FromRoute] string module_code, [FromQuery] PayrollType payrol_type,
            [FromQuery] Guid branch_id
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new CheckIfThereIsPayrollInProgressQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                BranchId = branch_id,
                PayrollType = payrol_type,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        [Tags("Nomina")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/payrolls")]      
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CreatedResult> InitializePayrollProcessAsync([FromRoute] Guid companie_id,  [FromRoute] string module_code, [FromBody] InitializePayrollProcessCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            //Solo administradores pueden aperturar ciclo de nomina.

            await _mediator.Send(new InitializePayrollProcessCommand(){
                CompanyId = companie_id,
                ModuleCode = module_code,
                BranchId = payload.BranchId,
                Type = payload.Type,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Created(string.Empty, null);
        }

        //Cerrar proceso de nomina
        [Tags("Nomina")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/payrolls/{payroll_id}/close")]      
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CreatedResult> ClosePayrollProcessAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] Guid payroll_id, [FromBody] ClosePayrollProcessCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = companie_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.PayrollId = payroll_id;
            
            await _mediator.Send(payload);

            return Created(string.Empty, null);
        }

        //Obtener detalles de la nomina activa en proceso.
        [Tags("Nomina")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/payrolls", Name = "GetPayrollActive")]      
        [ProducesResponseType(typeof(PayrollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PayrollDto> GetCurrentPayrollInProgressAsync([FromRoute] Guid companie_id,  [FromRoute] string module_code, 
            [FromQuery] PayrollType type,
            [FromQuery] Guid branch_id,
            [FromQuery] string? identification_number,
            [FromQuery] int? work_area_id,
            [FromQuery] int? job_position_id,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetCurrenPayrollInProgresssQuery(){
                CompanyId = companie_id,
                ModuleCode = module_code,
                Type = type,
                UserId = Guid.Parse(userIdStr ?? ""),
                BranchId = branch_id,
                PageNumber = page_number,    
                PageSize = page_size,
                IdentificationNumber = identification_number,
                WorkAreaId = work_area_id,
                WorkPositionId = job_position_id
            });

            return result;
        }

        //Historial de periodos cerrados de planillas
        [Tags("Nomina")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/branches/{branch_id}/payrolls")]      
        [ProducesResponseType(typeof(List<PayrollPeriodDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<List<PayrollPeriodDto>> ObtainPayrollPeriodsAsync(
            [FromRoute] Guid companie_id,
            [FromRoute] Guid branch_id,
            [FromQuery] PayrollType type,
            [FromRoute] string module_code, 

            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new ObtainPayrollPeriodsQuery()
            {
                Type = type,
                BrachId = branch_id,
                CompanyId = companie_id,
                ModuleCode = module_code,

                PageSize = page_size,
                PageNumber = page_number,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }

        //Obtener detalles de una nomina de algun periodo cerrado.
        [Tags("Nomina")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/branches/{branch_id}/payrolls/{payroll_id}/history")]      
        [ProducesResponseType(typeof(PayrollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
            [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> GetPayrollDetailsByIdAsync(
            [FromRoute] Guid branch_id,
            [FromRoute] Guid payroll_id,
            [FromRoute] Guid companie_id,
            [FromRoute] string module_code,

            [FromQuery] int? work_area_id,
            [FromQuery] int? job_position_id,
            [FromQuery] string? IdentificationNumber,

            [FromQuery] int page_size = 10,
            [FromQuery] int page_number = 1
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            //Solo administradores pueden aperturar ciclo de nomina.

            await _mediator.Send(new GePayrollDetaillsQuery()
            {
                BranchId = branch_id,
                PayrollId = payroll_id,
                CompanyId = companie_id,
                ModuleCode = module_code,
                
                WorkAreaId = work_area_id,
                WorkPositionId = job_position_id,
                IdentificationNumber = IdentificationNumber,

                PageSize = page_size,
                PageNumber = page_number,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Ok();
        }
    }
}