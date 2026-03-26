using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Services;
using ERP.Core.Manager.Api.Infrastructure.Persistence;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Database;

using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configuracion de la cadena de conexión de base de datos.
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var dataSource = NpgsqlConfiguration.BuildDataSource(connectionString!);
            
            services.AddSingleton(dataSource);
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(dataSource,
                    m => m.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            //Other Services.
            services.AddSingleton<ICodeGenerator, CodeGenerator>();
            services.AddSingleton<IAuthServices, AuthServices>();
            services.AddTransient<IErrorManager, ErrorManager>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            //Services
            services.AddScoped<IAuthServices, AuthServices>();

            //Repositories
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IUserProfilesRepository, UserProfilesRepository>();
            services.AddScoped<ISessionsRepository, SessionsRepository>();
            services.AddScoped<ICompaniesRepository, CompaniesRepository>();
            services.AddScoped<IModulesRepository, ModulesRepository>();
            services.AddScoped<IUserModulesRoleRepository, UserModulesRoleRepository>();
            services.AddScoped<IRolesRepository, RolesRepository>();
            services.AddScoped<ICollaboratorsRepository, CollaboratorsRepository>();


            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}