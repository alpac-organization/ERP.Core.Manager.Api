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


        [Tags("Nomina")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/payrolls", Name = "GetPayrollActive")]      
        [ProducesResponseType(typeof(PayrollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PayrollDto> GetCurrentPayrollInProgressAsync([FromRoute] Guid companie_id,  [FromRoute] string module_code, 
            [FromQuery] PayrollType type,
            [FromQuery] Guid branch_id,
            [FromQuery] int page_number = 1,
            [FromQuery] int page_size = 10
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;
            //Solo administradores pueden aperturar ciclo de nomina.

            var result = await _mediator.Send(new GetCurrenPayrollInProgresssQuery(){
                CompanyId = companie_id,
                ModuleCode = module_code,
                Type = type,
                UserId = Guid.Parse(userIdStr ?? ""),
                BranchId = branch_id,
                PageNumber = page_number,    
                PageSize = page_size
            });

            return result;
        }
    }
}