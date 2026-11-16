using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories;

namespace ERP.Core.Manager.Api.Application.Commons.Utils
{
    /// <summary>
    /// Días laborales de suspensión (sin domingo; sábado según DoesWorkSaturdays)
    /// y fechas Applied que solapan un periodo de nómina.
    /// </summary>
    public static class SuspensionUnpaidDates
    {
        public static bool IsLaborDay(DateOnly date, bool doesWorkSaturdays)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            if (date.DayOfWeek == DayOfWeek.Saturday && !doesWorkSaturdays)
                return false;

            return true;
        }

        public static HashSet<DateOnly> Enumerate(DateOnly from, DateOnly to, bool doesWorkSaturdays)
        {
            var dates = new HashSet<DateOnly>();
            if (to < from)
                return dates;

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (IsLaborDay(date, doesWorkSaturdays))
                    dates.Add(date);
            }

            return dates;
        }

        public static int Count(DateOnly from, DateOnly to, bool doesWorkSaturdays)
            => Enumerate(from, to, doesWorkSaturdays).Count;

        public static async Task<HashSet<DateOnly>> GetAsync(
            IUnitOfWork unitOfWork,
            Guid collaboratorId,
            DateOnly payrollStart,
            DateOnly payrollEnd,
            bool doesWorkSaturdays,
            CancellationToken cancellationToken = default)
        {
            var suspensions = await unitOfWork.DisciplinarySanctions.Entities
                .AsNoTracking()
                .Where(s =>
                    s.SanctionStatus == SanctionStatus.Applied &&
                    s.SanctionType == SanctionType.Suspension &&
                    s.DeletedAt == null &&
                    s.Fault.CollaboratorId == collaboratorId &&
                    s.SuspensionEndDate.HasValue &&
                    s.SuspensionStartDate.HasValue &&
                    s.SuspensionStartDate <= payrollEnd &&
                    s.SuspensionEndDate >= payrollStart)
                .Select(s => new
                {
                    Start = s.SuspensionStartDate!.Value,
                    End = s.SuspensionEndDate!.Value
                })
                .ToListAsync(cancellationToken);

            var suspensionDates = new HashSet<DateOnly>();

            foreach (var suspension in suspensions)
            {
                var from = suspension.Start > payrollStart ? suspension.Start : payrollStart;
                var to = suspension.End < payrollEnd ? suspension.End : payrollEnd;

                foreach (var date in Enumerate(from, to, doesWorkSaturdays))
                    suspensionDates.Add(date);
            }

            return suspensionDates;
        }
    }
}
