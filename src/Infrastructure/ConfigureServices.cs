using ERP.Core.Manager.Api.Domain.Interfaces.Repositories;
using ERP.Core.Manager.Api.Infrastructure.Persistence;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Context;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories;

using ERP.Core.Manager.Api.Domain.Interfaces;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Authentication;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ERP.Core.Manager.Api.Infrastructure.Services;
using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Interfaces.Repositories.Payroll;
using ERP.Core.Manager.Api.Infrastructure.Persistence.Repositories.Payroll;

using ERP.Core.Database.Domain.Enums;
using ERP.Core.Application.Commons.Interfaces;
using ERP.Core.Infrastructure.Services;

namespace ERP.Core.Manager.Api.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //Configuracion de la cadena de conexión de base de datos.
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsqlOptions.MapEnum<CatalogType>("catalog_type_enum");
                    npgsqlOptions.MapEnum<RoleType>("role_type_enum");
                    npgsqlOptions.MapEnum<PermissionType>("permission_type_enum");
                    npgsqlOptions.MapEnum<UserType>("user_type_enum");
                    npgsqlOptions.MapEnum<UserStatus>("user_status_enum");
                    npgsqlOptions.MapEnum<GenderType>("gender_type_enum");
                    npgsqlOptions.MapEnum<IdentificationType>("identification_type_enum");
                    npgsqlOptions.MapEnum<CollaboratorStatus>("collaborator_status_enum");
                    npgsqlOptions.MapEnum<SalaryType>("salary_type_enum");
                    npgsqlOptions.MapEnum<Currency>("currency_enum");
                    npgsqlOptions.MapEnum<PermitApplicationStatus>("permit_application_status_enum");
                    npgsqlOptions.MapEnum<PermitApplicationType>("permit_application_type_enum");
                    npgsqlOptions.MapEnum<MaritalStatus>("marital_status_enum");
                    npgsqlOptions.MapEnum<DeductionType>("deduction_type_enum");
                    npgsqlOptions.MapEnum<PayrollStatus>("payroll_status_enum");
                }));

            //Other Services del paquete de la empresa.
            services.AddSingleton<ICodeGenerator, CodeGenerator>();
            services.AddTransient<IErrorManager, ErrorManager>();

            services.AddTransient<ITemplateServices, TemplateServices>();
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
            services.AddScoped<ICatalogsRepository, CatalogsRepository>();
            services.AddScoped<ISubCatalogsRepository, SubCatalogsRepository>();
            services.AddScoped<IPersonalInformationRepository, PersonalInformationRepository>();
            services.AddScoped<IWorkingInformationRepository, WorkingInformationRepository>();
            services.AddScoped<ISalariesRepository, SalariesRepository>();
            services.AddScoped<IVacationsRepository, VacationsRepository>();
            services.AddScoped<IPermitApplicationsRepository, PermitApplicationsRepository>();
            services.AddScoped<IDeductionsRepository, DeductionsRepository>();
            services.AddScoped<IPayrollsRepository, PayrollsRepository>();
            services.AddScoped<IOrdinaryPayrollsRepository, OrdinaryPayrollsRepository>();
            services.AddScoped<IWorkPositionsHistoryRepository, WorkPositionsHistoryRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}