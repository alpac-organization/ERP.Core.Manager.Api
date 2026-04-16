using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class PayrollsController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Deducciones")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/payrolls-status")]      
        [ProducesResponseType(typeof(CheckPayrollDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<CheckPayrollDto> CheckIfThereIsPayrollInProgressAsync([FromRoute] Guid companie_id,  [FromRoute] string module_code, [FromQuery] PayrollType payrol_type)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new CheckIfThereIsPayrollInProgressQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                PayrollType = payrol_type,
                UserId = Guid.Parse(userIdStr ?? "")
            });
        }
    }
}