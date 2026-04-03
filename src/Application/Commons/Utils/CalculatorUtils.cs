namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    public static class CalculatorUtils
    {
        /// <summary>
        /// Calcula los días transcurridos entre dos fechas (equivalente a tu función TS).
        /// </summary>
        public static int CalculateDaysElapsedCommercial(DateTime entryDate)
        {
            var start = entryDate.Date;
            var today = DateTime.UtcNow.Date;

            if (today < start) return 0;

            int days = 0;
            DateTime current = start;

            while (current < today)
            {
                if (current.Day == 31)
                {
                    current = current.AddDays(1);
                    continue;
                }

                if (current.Month == 2 && current.Day == DateTime.DaysInMonth(current.Year, 2))
                {
                    int adjustment = 30 - current.Day; 
                    days += (1 + adjustment);

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