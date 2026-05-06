using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Infrastructure.Services;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Infrastructure.Services;
using ERP.Core.Manager.Api.Infrastructure.Schedules;
using ERP.Core.Database.Infrastructure;

namespace ERP.Core.Manager.Api.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configuracion de la cadena de conexión de base de datos.
            //Other Services del paquete de la empresa.
            services.AddSingleton<ICodeGenerator, CodeGenerator>();
            services.AddTransient<IErrorManager, ErrorManager>();

            services.AddTransient<ITemplateServices, TemplateServices>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ICalculatorDeductions, CalculatorDeductions>();
            
            services.AddScoped<ITemplateServices, TemplateServices>();
            services.AddScoped<IPdfGeneratorServices, PdfGeneratorServices>();
            services.AddScoped<IAuthServices, AuthServices>();

            // services.AddJobScheduling();
            services.AddErpDatabaseServices(configuration);

            return services;
        }
    }
}