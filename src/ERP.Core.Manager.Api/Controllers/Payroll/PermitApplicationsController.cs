using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Infrastructure.Attributes;

using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.PermitApplication.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class PermitApplicationsController(IMediator _mediator) : ApiControllerBase
    {

        //✅Registro de una nueva solicitud de trabajo.
        [Tags("Permisos")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/permit-applications")]      
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]     
        public async Task<CreatedResult> CreatePermitApplicationAsync(
            [FromRoute] Guid companie_id, 
            [FromRoute] string module_code,
            [FromBody] CreatePermitApplicationCommand Payload 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            Payload.CompanyId = companie_id;
            Payload.ModuleCode = module_code;
            Payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(Payload);

            return Created();
        }

        //✅Obtener permisos solicitados con queries establecidas para cualquier area
        [Tags("Permisos")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/permit-applications")]      
        [ProducesResponseType(typeof(PagedResponse<PermitApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<PagedResponse<PermitApplicationDto>> GetPermitApplicationsAsync([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromQuery] int page_size                   = 10, 
            [FromQuery] int page_number                 = 1, 
            [FromQuery] Guid? payroll_id                = null,
            [FromQuery] string? identification_number   = null,
            [FromQuery] PermitApplicationType? type     = null,
            [FromQuery] PermitApplicationStatus? status = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetPermitApplicationsQuery()
            {
                Type = type,
                Status = status,
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                PayrollId  = payroll_id,
                IdentificationNumber = identification_number,
                PageSize = page_size,
                PageNumber = page_number
            });            
        }

        //❌Actualizar solicitud de permiso. pendiente.
        [Tags("Permisos")] 
        [HttpPut("companies/{companie_id}/modules/{module_code}/permit-applications/{permit_application_id}")]      
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<OkResult> UpdatePermitApplicationAsync([FromRoute] Guid companie_id, [FromRoute] string module_code,
            [FromRoute] Guid permit_application_id
            // [FromBody] CuerpoActualización 
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

           return Ok();          
        }
        
        //✅Procesar solicitud de permiso, en este caso aprobar steps, seconds steps
        [Tags("Permisos")] 
        [HttpPost("companies/{companie_id}/modules/{module_code}/permit-applications/{permit_application_id}/process")]      
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]   
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> ProcessVacationRequestAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] Guid permit_application_id,
        [FromBody] ProcessPermitApplicationCommand Payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new ProcessPermitApplicationCommand()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                PermitApplicationId = permit_application_id,
                IsApproved = Payload.IsApproved
            });

            return Ok();            
        }


        //✅Cancelar solicitud de permisos
        [Tags("Permisos")] 
        [HttpGet("companies/{companie_id}/modules/{module_code}/permit-applications/{permit_application_id}/abort")]      
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]  
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]  
        public async Task<IActionResult> CancelPermitRequestAsync([FromRoute] Guid companie_id, [FromRoute] string module_code, [FromRoute] Guid permit_application_id)
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new CancelPermitRequestQuery()
            {
                CompanyId = companie_id,
                ModuleCode = module_code,
                PermitApplicationRequestId = permit_application_id,
                UserId = Guid.Parse(userIdStr ?? "")
            });

            return Ok();            
        }
    }
}