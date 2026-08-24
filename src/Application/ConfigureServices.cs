using MediatR;
using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Application.Behaviors;
using ERP.Core.Manager.Api.Application.Commons.Options;

// using ERP.Core.Manager.Api.Application.Behaviors;

namespace ERP.Core.Manager.Api.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddAutoMapper(typeof(ConfigureServices).Assembly);
            services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
            
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            services.Configure<NotificationsOptions>(configuration.GetSection(NotificationsOptions.SectionName));

            return services;
        }
    }
}