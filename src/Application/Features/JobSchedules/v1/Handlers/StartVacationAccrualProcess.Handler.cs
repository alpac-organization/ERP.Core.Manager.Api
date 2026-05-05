using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Features.JobSchedules.v1.Commands;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Features.JobSchedules.v1.Handlers
{
    public class StartVacationAccrualProcessHandler(IUnitOfWork _unitOfWork, ILogger<StartVacationAccrualProcessHandler> _logger) : IRequestHandler<StartVacationAccrualProcessCommand>
    {
        public async Task Handle(StartVacationAccrualProcessCommand request, CancellationToken cancellationToken)
        {
            #pragma warning disable CA1873
            
            _logger.LogInformation("Iniciando proceso automático de acumulación de vacaciones: {Time}", DateTime.Now);
            
            // #pragma warning restore CA1873

            decimal daysToAdd = 0.0833m;

            var collaborators = await _unitOfWork.Collaborators.Entities
                .Include(c => c.Salaries
                    .Where(s => s.EndDate == null && (s.SalaryType == SalaryType.Fixed || s.SalaryType == SalaryType.Variable))
                )
                .Include(c => c.WorkingInformation)
                .Where(c => 
                    c.Status != CollaboratorStatus.Inactive || c.Status != CollaboratorStatus.Subsidy
                )
                .Where(c => c.Salaries
                    .Any(s => s.EndDate == null && (s.SalaryType == SalaryType.Fixed || s.SalaryType == SalaryType.Variable))
                )
                .ToListAsync(cancellationToken);

            foreach(var collaborator in collaborators)
            {
                var vacationControl = await _unitOfWork.Vacations.Entities
                    .Where(col => col.CollaboratorId == collaborator.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (vacationControl is null)
                {
                    _logger.LogInformation("No se encontrol registro de vacaciones, para el colaborador con cedula: {cedula}", collaborator.IdentificationNumber);
                    continue;
                }

                vacationControl.GeneredVacation += daysToAdd;
                vacationControl.AvailableVacations += daysToAdd;

                await _unitOfWork.Vacations.UpdateAsync(vacationControl);

                _logger.LogInformation("Vacaciones actualizadas correctamente para colaborador con cedula: {cedula}", collaborator.IdentificationNumber);

            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);


            #pragma warning restore CA1873
            return;
        }
    }
}