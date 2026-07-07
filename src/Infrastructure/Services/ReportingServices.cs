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

        public async Task ApplyVacationRegistration(Collaborator collaborator, Guid payrollId)
        {

        }

        public async Task ApplyInssReporting(string period, Guid payrollId, Collaborator collaborator, decimal income, decimal inssLabor)
        {
            int countCollaborators = await _unitOfWork.Collaborators.Entities
                .Where(col => col.Status != CollaboratorStatus.Inactive)
                .Where(col => !col.DeletedAt.HasValue)
                .CountAsync(c => c.CompanyId == collaborator.CompanyId);

            var validDeductions = await _unitOfWork.ValidityDeductions.Entities
                .Where(v => v.Status)
                .ToListAsync(default);

            decimal inatecPercentage = validDeductions.FirstOrDefault(d => d.Type == TaxType.Inatec)?.Value ?? 0.02m;

            decimal inssPatronalPercentage = countCollaborators >= 50
                ? validDeductions.FirstOrDefault(d => d.Type == TaxType.InssPatronal)?.Value ?? 0.225m
                : validDeductions.FirstOrDefault(d => d.Type == TaxType.InssPatronal2)?.Value ?? 0.215m;

            decimal inssLaboralCalc = Math.Round(inssLabor, 2, MidpointRounding.AwayFromZero);
            decimal inatecCalc = Math.Round(income * inatecPercentage, 2, MidpointRounding.AwayFromZero);
            decimal inssPatronalCalc = Math.Round(income * inssPatronalPercentage, 2, MidpointRounding.AwayFromZero);

            decimal total = inssLaboralCalc + inatecCalc + inssPatronalCalc;
            decimal incomeRounded = Math.Round(income, 2, MidpointRounding.AwayFromZero);

            var existingRecord = await _unitOfWork.InssAccountingInformation.Entities
                .Where(x => x.PayrollId == payrollId && x.CollaboratorId == collaborator.Id)
                .FirstOrDefaultAsync(default);

            if (existingRecord is null)
            {
                await _unitOfWork.InssAccountingInformation.RegisterInssAccountingInformation(new InssAccountingInformation()
                {
                    CollaboratorId = collaborator.Id,
                    PayrollId = payrollId,
                    InssLabor = inssLaboralCalc,
                    Inatec = inatecCalc,
                    InssPatronal = inssPatronalCalc,
                    Total = total,
                    Absence = 0,
                    DaysAbsence = 0,
                    Income = incomeRounded
                });
            }
            else
            {
                existingRecord.InssLabor = inssLaboralCalc;
                existingRecord.Inatec = inatecCalc;
                existingRecord.InssPatronal = inssPatronalCalc;
                existingRecord.Total = total;
                existingRecord.Income = incomeRounded;

                await _unitOfWork.InssAccountingInformation.UpdateAsync(existingRecord);
            }
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

            if (vacationAccruals is null)
            {
                _logger.LogInformation("No se encontro registro del control de reporte: {identfication}", collaborator.IdentificationNumber);
                return;
            }

            var salary = await _unitOfWork.Salaries.Entities
                .Where(sal => sal.CollaboratorId == collaborator.Id)
                .Where(sal => sal.EndDate == null)
                .FirstOrDefaultAsync();

            if (salary is null)
            {
                _logger.LogInformation("No se encontro el registro salarial del colaborador con cedula: {identfication}", collaborator.IdentificationNumber);
                return;
            }

            decimal MonthlySalary = salary.AmountInLocal;
            decimal DailySalary = MonthlySalary / 30;

            decimal newEquivalent = vacationControl.AvailableVacations * DailySalary;

            vacationAccruals.FinalBalance = vacationControl.AvailableVacations;
            vacationAccruals.EquivalentQuantity = newEquivalent;
            vacationAccruals.EquivalentQuantityInDollars = newEquivalent / exchangeRate.Value;

            await _unitOfWork.VacationAccruals.UpdateAsync(vacationAccruals);
        }

    }
}