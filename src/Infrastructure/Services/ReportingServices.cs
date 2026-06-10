using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

using ERP.Core.Manager.Api.Application.Commons.Interfaces;
namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    //Administrador de reportes de nomina.
    public class ReportingServices(IUnitOfWork _unitOfWork, ILogger<ReportingServices> _logger) : IReportingServices
    {

        public async Task RegisterChrismasBonus(Collaborator collaborator, Guid payrollId)
        {
            // await _unitOfWork.   
        }

        public async Task ApplyVacationMovement(Collaborator collaborator, Guid payrollId)
        {
            //Obtener la mesa de cambio oficial
            var exchangeRate = await _unitOfWork.ValidityDeductions.Entities
                .Where(val => val.Status)
                .Where(val => val.Type == TaxType.ExchangeRate)
                .FirstOrDefaultAsync(default);

            if (exchangeRate is null)
            {
                _logger.LogInformation("No fue posible obtener la mesa de cambio");
                return;
            }

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

            var salary = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.CollaboratorId == collaborator.Id)
                .Where(sal => sal.EndDate == null)
                .FirstOrDefaultAsync();

            if(salary is null)
            {
                _logger.LogInformation("No se encontro el registro salarial del colaborador con cedula: {identfication}", collaborator.IdentificationNumber);
                return;
            }

            decimal MonthlySalary = salary.AmountInLocal;
            decimal DailySalary  = MonthlySalary / 30;

            decimal newEquivalent = vacationControl.AvailableVacations * DailySalary;

            vacationAccruals.FinalBalance = vacationControl.AvailableVacations;
            vacationAccruals.EquivalentQuantity = newEquivalent;
            vacationAccruals.EquivalentQuantityInDollars = newEquivalent / exchangeRate.Value;

            await _unitOfWork.VacationAccruals.UpdateAsync(vacationAccruals);
        }
    
        public async Task ApplyVacationRegistration(Collaborator collaborator, Guid payrollId)
        {
            
        }
    }
}