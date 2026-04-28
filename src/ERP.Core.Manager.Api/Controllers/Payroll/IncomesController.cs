using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Deductions.v1.Queries;
using ERP.Core.Manager.Api.Application.Features.Incomes.v1.Commands;
using PuppeteerSharp;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class IncomesController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Ingresos")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/incomes")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterIncomeAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromBody] RegisterIncomeCommand payload)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new RegisterIncomeCommand()
            {
                AmountHours = payload.AmountHours,
                CompanyId = companie_id,
                IdentificationNumber = payload.IdentificationNumber,
                IncomeAmount = payload.IncomeAmount,
                ModuleCode = module_code,
                TypeIncomeId = payload.TypeIncomeId,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Ok();
        }
    }
}