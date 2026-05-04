using Quartz;
using MediatR;
using ERP.Core.Manager.Api.Application.Features.JobSchedules.v1.Commands;

namespace ERP.Core.Manager.Api.Infrastructure.Schedules.Jobs
{
    [DisallowConcurrentExecution]
    public class VacationAccrualJob(ISender _mediator): IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await _mediator.Send(new StartVacationAccrualProcessCommand());
        }
    }
}