using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Services;
using ERP.Core.Manager.Api.Infrastructure.Persistence;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;

namespace ERP.Core.Manager.Api.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configuracion de la cadena de conexión de base de datos.
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString,
                    m => m.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
            
            var dataSource = NpgsqlConfiguration.BuildDataSource(connectionString!);

            //Other Services.
            services.AddSingleton(dataSource);
            services.AddSingleton<ICodeGenerator, CodeGenerator>();


            //Services
            services.AddScoped<IAuthServices, AuthServices>();


            //Repositories
            services.AddScoped<ICompaniesRepository, CompaniesRepository>();
            services.AddScoped<IModulesRepository, ModulesRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}