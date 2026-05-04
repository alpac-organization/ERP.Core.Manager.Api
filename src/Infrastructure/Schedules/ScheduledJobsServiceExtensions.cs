using Quartz;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Infrastructure.Schedules.Jobs;
using System.Runtime.InteropServices;

namespace ERP.Core.Manager.Api.Infrastructure.Schedules
{
    public static class ScheduledJobsServiceExtensions
    {
        public static IServiceCollection AddJobScheduling(this IServiceCollection services)
        {
            services.AddQuartz(quartz =>
            {
                string tzId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                    ? "Central Standard Time (Mexico)" 
                    : "America/Managua";

                var managuaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);

                #region Acumulador de vacaciones

                var jobKey = new JobKey("VacationAccumulator");
                quartz.AddJob<VacationAccrualJob>(opts => opts.WithIdentity(jobKey));

                quartz.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("VacationJob-trigger")
                    .WithCronSchedule("0 30 22 * * ?", x => x.InTimeZone(managuaTimeZone))
                );

                #endregion


            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            return services;
        }
    }
}