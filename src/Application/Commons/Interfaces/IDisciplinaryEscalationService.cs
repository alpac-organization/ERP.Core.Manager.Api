
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Application.Commons.Interfaces
{
   public interface IDisciplinaryEscalation
   {
      public Task<bool> ExecuteEscalationProcess(DisciplinaryFault disciplinaryFault, CancellationToken cancellationToken);

      public (DateOnly Start, DateOnly End) GetMonthBoundaries(DateOnly OccurrenceDate);
   }
}