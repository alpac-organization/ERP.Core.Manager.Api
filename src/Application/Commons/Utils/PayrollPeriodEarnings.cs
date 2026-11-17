namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    /// <summary>
    /// Contexto de devengo salarial de un periodo de nómina
    /// (días pagados vs no pagados → base para INSS/IR).
    /// </summary>
    public static class PayrollPeriodEarnings
    {
        public sealed record Result(
            int PeriodDays,
            int EarnedDays,
            int UnpaidDays,
            decimal GrossSalary,
            decimal TotalIncome,
            decimal UnpaidAmount);

        /// <param name="dailySalary">Salario mensual / 30</param>
        /// <param name="periodDays">Días del periodo (p. ej. daysWorked, máx 15)</param>
        /// <param name="unpaidDates">Días del periodo que no se pagan (suspensión, etc.)</param>
        public static Result ForPeriod(
            decimal dailySalary,
            int periodDays,
            IReadOnlyCollection<DateOnly> unpaidDates,
            decimal overtime = 0m,
            decimal commissions = 0m,
            decimal antique = 0m,
            decimal bonus = 0m)
        {
            if (periodDays < 0) periodDays = 0;

            int unpaidDays = unpaidDates?.Count ?? 0;
            if (unpaidDays > periodDays) unpaidDays = periodDays;

            int earnedDays = periodDays - unpaidDays;
            if (earnedDays < 0) earnedDays = 0;

            decimal grossSalary = dailySalary * earnedDays;
            decimal unpaidAmount = dailySalary * unpaidDays;
            decimal totalIncome = grossSalary + overtime + commissions + antique + bonus;

            return new Result(
                periodDays,
                earnedDays,
                unpaidDays,
                grossSalary,
                totalIncome,
                unpaidAmount);
        }
    }
}
