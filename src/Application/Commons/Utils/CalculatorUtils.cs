namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class CalculatorUtils
    {
        public static int CalculateDaysElapsedCommercial(DateOnly? entryDate)
        {

            if (!entryDate.HasValue)
                return 0;

            DateOnly start = entryDate.Value;
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < start)
                return 0;

            int days = 0;
            DateOnly current = start;

            while (current < today)
            {
                if (current.Day == 31)
                {
                    current = current.AddDays(1);
                    continue;
                }

                if (current.Month == 2 &&
                    current.Day == DateTime.DaysInMonth(current.Year, 2))
                {
                    int adjustment = 30 - current.Day;

                    days += 1 + adjustment;

                    current = current.AddDays(1);
                }
                else
                {
                    days++;
                    current = current.AddDays(1);
                }
            }

            return days;
        }
    }
}