using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Bases;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Commands;

namespace ERP.Core.Manager.Api.Application.Features.Collaborators.v1.Handlers
{
    public class DeactivateCollaboratorHandler(IUnitOfWork _unitOfWork, IErrorManager _errorManager, ILogger<DeactivateCollaboratorHandler> _logger): BaseValidatorHandler<DeactivateCollaboratorCommand, bool>(_unitOfWork, _errorManager)
    {
        public override async Task<bool> Handle(DeactivateCollaboratorCommand request, CancellationToken cancellationToken)
        {
            
            var access = await ValidateAccessAsync(request.UserId, request.CompanyId, request.ModuleCode!, cancellationToken);

            if (!access.IsSuccess) 
            {
                return access.ErrorResponse; 
            }

            _logger.LogInformation("🚀Iniciando proceso de baja colaborador.");

            var collaborator = await _unitOfWork.Collaborators.Entities
                .Where(col => col.CompanyId == request.CompanyId)
                .Where(col => col.IdentificationNumber == request.IdentificationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (collaborator is null)
            {
                return _errorManager.ThrowBadRequest<bool>("Este colaborador no existe en nuestro sitema,", "ERP:01");
            }

            if (collaborator.Status == CollaboratorStatus.Inactive)
            {
                return _errorManager.ThrowBadRequest<bool>("Este colaborador ya ha sido dado de baja", "ERP:02");
            }   

            //Actualizar información
            collaborator.HasBeenFired = true;
            collaborator.DeletedAt = DateTime.Now;
            collaborator.Status = CollaboratorStatus.Inactive;

            //Actualizar información salarial
            var lastSalary = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(cancellationToken);


            if (lastSalary is null)
            {
                _logger.LogInformation("No se encontro la información salarial del colaborador: {identification}", collaborator.IdentificationNumber);
                return _errorManager.ThrowBadRequest<bool>("No logramos procesar la baja del colaborador", "ERP:02");
            }
            
            //Finalizamos su historial de salarios
            lastSalary.EndDate = DateTime.Now;

            await _unitOfWork.Salaries.UpdateAsync(lastSalary);

            var lastWorkposition = await _unitOfWork.WorkPositionHistories.Entities
                .Where(his => his.EndDate == null)
                .Where(his => his.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastWorkposition is not null)
            {
                lastWorkposition.EndDate = DateTime.Now;
                await _unitOfWork.WorkPositionHistories.UpdateAsync(lastWorkposition);        
            }

            //Verificar si dejo algún progreso de solicitudes activas
            var permitApplications = await _unitOfWork.PermitApplications.Entities
                .Where(per => per.CollaboratorId == collaborator.Id)
                .Where(per => per.Status == PermitApplicationStatus.Pending)
                .ToListAsync(cancellationToken);

            //Cerramos cualquier tipo de solicitud del colaborador
            foreach (var permit in permitApplications)
            {
                permit.Status = PermitApplicationStatus.Cancelled;
                await _unitOfWork.PermitApplications.UpdateAsync(permit);
            }

            // var deductions = await _unitOfWork.Deductions.Entities
            //     .Where(deduction => deduction.CollaboratorId == collaborator.Id)
            //     .Include(deduction => deduction)
            //     .ToListAsync(cancellationToken);

            // foreach(var deduction in deductions)
            // {
            //     if (deduction.Type == DeductionType.)
            //     {
                    
            //     }
            // }

            await _unitOfWork.Collaborators.UpdateAsync(collaborator);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅Colaborador dado de baja con exito del sistema");

            return true;
        }
    }
}