using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Infrastructure.Services;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Infrastructure.Services; 
using ERP.Core.Database.Infrastructure;
using ERP.Core.Database.Application.Commons.Interfaces.Repositories.Payrolls;
using ERP.Core.Database.Infrastructure.Persistence.Repositories.Payroll;

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
            
            //Servicios de deduciones e ingresos colaborador.
            services.AddScoped<IIncomeServices, IncomeServices>();
            services.AddScoped<IPayrollServices, PayrollServices>();
            services.AddScoped<IReportingServices, ReportingServices>();
            services.AddScoped<IDeductionsServices, DeductionsServices>();
            
            services.AddScoped<ITemplateServices, TemplateServices>();
            services.AddScoped<IPdfGeneratorServices, PdfGeneratorServices>();
            services.AddScoped<IAuthServices, AuthServices>();

            // services.AddJobScheduling();
            services.AddErpDatabaseServices(configuration);

            return services;
        }
    }
}