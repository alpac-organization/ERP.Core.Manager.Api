using ERP.Core.Database.Application.Commons.Interfaces.Repositories;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    //Administrador de reportes de nomina.
    public class ReportingServices(IUnitOfWork _unitOfWork, ILogger<ReportingServices> _logger) : IReportingServices
    {
        public async Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId)
        {
            var vacationControl = await _unitOfWork.Vacations.Entities
                .Where(vac => vac.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(default);

            if (vacationControl is null)
            {
                _logger.LogInformation("No se encontro el control de vacaciones para la actualización de reporte: {identfication}", collaborator.IdentificationNumber);
                return;
            }

            var vacationAccruals = await _unitOfWork.VacationAccruals.Entities
                .Where(acr => acr.PayrollId == payrollId)
                .Where(acr => acr.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(default);

            if(vacationAccruals is null)
            {
                _logger.LogInformation("No se encontro registro del control de reporte: {identfication}", collaborator.IdentificationNumber);
                return;
            }
        }
    }
}