using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class CalculatorDeductions(IUnitOfWork _unitOfWork) : ICalculatorDeductions
    {
        public decimal CalculateInss(decimal Salary)
        {
            decimal inss = Salary * 0.07m;
            return inss;
        }

        //Salario Mensual = MonthlySalary, 
        public decimal CalculateIR(decimal GrossSalary, int DaysWorked)
        {
            





            return 0;
        }

        public async Task RegisterOrdinaryPayrollForCollaborator(Guid PayrollId, Guid CollaboratorId, CancellationToken cancellationToken)
        {
            //Obtener detalles de la nomina
            var payrollCreated = await _unitOfWork.Payrolls.Entities
                .Where(payroll => payroll.Id == PayrollId)
                .FirstOrDefaultAsync(cancellationToken);

            if (payrollCreated is null)
            {
                return;
            }

            var salary = await _unitOfWork.Salaries.Entities 
                .Include(salary => salary.Collaborator) 
                    .ThenInclude(salary => salary.WorkingInformation)
                .Where(salary => salary.EndDate == null)
                .Where(salary => salary.SalaryType == SalaryType.Fixed)
                .Where(salary => salary.CollaboratorId == CollaboratorId)
                .FirstOrDefaultAsync(cancellationToken);

            if (salary is null)
            {
                return;
            }

            //Constantes Salariales
            decimal MonthlySalary   = salary.AmountSalary;
            decimal BiweeklySalary  = MonthlySalary / 2;
            decimal DailySalary     = MonthlySalary / 30;

            //Buscar Registros de horas extras

            // await _unitOfWork.Overtimes.Entities.FindFirstOrDefaultAsync(cancellatonToken);
            decimal Overtime = 0.0m;

            //await CalculateOvertimes(CollaboratorId);

            //Buscar Registros de Bonos
            decimal Bonus = 0.0m;

            //await GetBonuses(CollaboratorId);

            //Salario Bruto, Sumamos Horas Extras, Bonos, Salario Quincenal
            decimal GrossSalary = BiweeklySalary + Overtime + Bonus;

            //Deducimos Inss regla del 0.07%, aqui es quincenal el salario. por lo tanto el inss es quincenal
            decimal InssBiweekly = CalculateInss(GrossSalary);

            //Verificar si es un colaborador nuevo que acaba de ingresar o ya es viejo
            DateTime EntryDate = salary.Collaborator.WorkingInformation.EntryDate;
            DateTime PayrollStartDate = payrollCreated.StartDate;

            // int StarndardDays = 15;
            // int ProportionalDays = 0;


            //Calculo de ir
            // decimal Ir = CalculateIR();

            //Calcular Deducciones, en esta caso ir a buscar deducciones activas del colaborador para deducir.
            decimal Deductions = 0.0m;

            decimal TotalLegalDeductions = Inss + Ir;

            decimal TotalDeductions = TotalLegalDeductions + Deductions;


            var payload = new OrdinaryPayroll()
            {
                CollaboratorId   = CollaboratorId,
                PayrollId        = PayrollId,

                GrossSalary      = GrossSalary,
                Bonus            = Bonus,
                Inss             = InssBiweekly,
                Ir               = 0.0m,
                Overtime         = Overtime,
                Deductions       = Deductions,
                TotalDeducctions = TotalDeductions,
                Vacations        = 0.0m,
            };

            await _unitOfWork.OrdinaryPayrolls.RegisterCollaboratorInTheOrdinaryPayroll(payload);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}