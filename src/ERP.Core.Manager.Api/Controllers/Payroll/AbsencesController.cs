using MediatR;
using Microsoft.AspNetCore.Mvc;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Domain.Entities.Errors;
using ERP.Core.Manager.Api.Controllers.ApiBase;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Dtos;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Commands;
using ERP.Core.Manager.Api.Application.Features.Absences.v1.Queries;
using ERP.Core.Manager.Api.Domain.Entities.Bases;
using ERP.Core.Infrastructure.Attributes;

namespace ERP.Core.Manager.Api.Controllers.Payroll
{
    [HasToken]
    [ApiVersion("1.0")]
    [Route("api/v1/")]
    public class AbsencesController(IMediator _mediator) : ApiControllerBase
    {
        // Registrar una nueva inasistencia (Jefe Inmediato o Sistema)
        [Tags("Ausencias")]
        [HttpPost("companies/{company_id}/modules/{module_code}/absences")]
        [ProducesResponseType(typeof(CreatedResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<CreatedResult> RegisterAbsenceAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromBody] RegisterAbsenceCommand payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");

            await _mediator.Send(payload);

            return Created();
        }

        // Aplicar inasistencia por RRHH (afecta nómina y disciplina)
        [Tags("Ausencias")]
        [HttpPost("companies/{company_id}/modules/{module_code}/absences/{absence_id}/apply")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> ApplyAbsenceAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromRoute] Guid absence_id,
            [FromBody] ApplyAbsenceCommand? payload = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var command = payload ?? new ApplyAbsenceCommand();
            command.CompanyId = company_id;
            command.ModuleCode = module_code;
            command.UserId = Guid.Parse(userIdStr ?? "");
            command.AbsenceId = absence_id;

            await _mediator.Send(command);

            return Ok();
        }

        // Cancelar inasistencia por RRHH (por acuerdo fuera del sistema: vacaciones, permiso, etc.)
        [Tags("Ausencias")]
        [HttpPost("companies/{company_id}/modules/{module_code}/absences/{absence_id}/cancel")]
        [ProducesResponseType(typeof(OkResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<OkResult> CancelAbsenceAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromRoute] Guid absence_id,
            [FromBody] CancelAbsenceCommand payload
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            payload.CompanyId = company_id;
            payload.ModuleCode = module_code;
            payload.UserId = Guid.Parse(userIdStr ?? "");
            payload.AbsenceId = absence_id;

            await _mediator.Send(payload);

            return Ok();
        }

        // Eliminar lógicamente (anular) inasistencia por error de registro
        [Tags("Ausencias")]
        [HttpDelete("companies/{company_id}/modules/{module_code}/absences/{absence_id}")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<NoContentResult> DeleteAbsenceAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromRoute] Guid absence_id
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            await _mediator.Send(new DeleteAbsenceCommand
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                AbsenceId = absence_id
            });

            return NoContent();
        }

        // Obtener listado de inasistencias con filtros para bandeja de RRHH
        [Tags("Ausencias")]
        [HttpGet("companies/{company_id}/modules/{module_code}/absences")]
        [ProducesResponseType(typeof(PagedResponse<AbsenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<PagedResponse<AbsenceDto>> GetAbsencesAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromQuery] int page_size = 10,
            [FromQuery] int page_number = 1,
            [FromQuery] Guid? branch_id = null,
            [FromQuery] Guid? collaborator_id = null,
            [FromQuery] string? identification_number = null,
            [FromQuery] AbsenceStatus? status = null,
            [FromQuery] AbsenceSource? source = null,
            [FromQuery] DateOnly? start_date = null,
            [FromQuery] DateOnly? end_date = null,
            [FromQuery] Guid? payroll_id = null
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            return await _mediator.Send(new GetAbsencesQuery
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                BranchId = branch_id,
                CollaboratorId = collaborator_id,
                IdentificationNumber = identification_number,
                Status = status,
                Source = source,
                StartDate = start_date,
                EndDate = end_date,
                PayrollId = payroll_id,
                PageSize = page_size,
                PageNumber = page_number
            });
        }

        // Obtener detalle plano de inasistencia por ID
        [Tags("Ausencias")]
        [HttpGet("companies/{company_id}/modules/{module_code}/absences/{absence_id}")]
        [ProducesResponseType(typeof(AbsenceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AbsenceDetailDto>> GetAbsenceByIdAsync(
            [FromRoute] Guid company_id,
            [FromRoute] string module_code,
            [FromRoute] Guid absence_id
        )
        {
            var userIdStr = HttpContext.Items["UserId"] as string;

            var result = await _mediator.Send(new GetAbsenceByIdQuery
            {
                CompanyId = company_id,
                ModuleCode = module_code,
                UserId = Guid.Parse(userIdStr ?? ""),
                AbsenceId = absence_id
            });

            return Ok(result);
        }
    }
}
