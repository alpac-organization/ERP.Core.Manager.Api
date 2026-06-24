using ERP.Core.Database.Domain.Entities.Payrolls;
using ERP.Core.Database.Domain.Enums;

namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class ManagerUtils
    {
        public static string FromSliceToCollaboratorFullname(Collaborator collaborator)
        {
            var fullNames = new[]
            {
                collaborator.FirstName,
                collaborator.SecondName,
                collaborator.ThirdName,
                collaborator.FirstLastname,
                collaborator.SecondLastname
            };

            return string.Join(" ", fullNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n?.Trim()));
        }

        public static (DateOnly, DateOnly, PayrollPeriod period) DefineRegularPayrollOpeningDates(Payroll? payroll)
        {
            DateOnly startDate;
            DateOnly endDate;
            PayrollPeriod period;
            if (payroll == null)
            {
                DateOnly today = DateOnly.FromDateTime(DateTime.Now);

                if (today.Day <= 15)
                {
                    startDate = new DateOnly(today.Year, today.Month, 1);
                    endDate = new DateOnly(today.Year, today.Month, 15);
                    period = PayrollPeriod.FirstPeriod;
                }
                else
                {
                    startDate = new DateOnly(today.Year, today.Month, 16);

                    endDate = DateOnly.FromDateTime(
                        new DateTime(today.Year, today.Month, 1)
                            .AddMonths(1)
                            .AddDays(-1));
                    period = PayrollPeriod.SecondPeriod;
                }
            }
            else
            {
                DateOnly lastEnd = payroll.EndDate;

                if (lastEnd.Day == 15)
                {
                    startDate = lastEnd.AddDays(1);

                    endDate = DateOnly.FromDateTime(
                        new DateTime(lastEnd.Year, lastEnd.Month, 1)
                            .AddMonths(1)
                            .AddDays(-1));
                    period = PayrollPeriod.SecondPeriod;
                }
                else
                {
                    startDate = lastEnd.AddDays(1);

                    endDate = new DateOnly(
                        startDate.Year,
                        startDate.Month,
                        15);
                    period = PayrollPeriod.FirstPeriod;
                }
            }

            return (startDate, endDate, period);
        }

    }
}
