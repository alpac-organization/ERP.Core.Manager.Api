using MediatR;
using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using ERP.Core.Manager.Api.Application.Behaviors;

namespace ERP.Core.Manager.Api.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddAutoMapper(typeof(ConfigureServices).Assembly);
            services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly);
            
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });


            return services;
        }
    }
}