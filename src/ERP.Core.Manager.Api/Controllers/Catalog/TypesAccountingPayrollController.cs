using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Payroll.v1.Queries;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Catalog
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class TypesAccountingPayrpollController(IMediator _mediator) : ApiControllerBase
    {
        [Tags("Catalogos")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/types-accounting-payroll")]      
        [ProducesResponseType(typeof(List<TypesAccountingPayrollDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<List<TypesAccountingPayrollDto>> GetTypesAccountingPayrpollAsync([FromRoute] Guid companie_id, [FromRoute] string module_code)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(
                new GetTypesAccountingPayrollQuery()
                {
                    CompanyId = companie_id,
                    ModuleCode = module_code,
                    UserId = Guid.Parse(userIdStr ?? "")
                }
            );
        }

    }
}
