using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Auth;
using ERP.Core.Database.Domain.Entities.Catalogs;
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<UserProfile> Profiles => Set<UserProfile>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<UserModuleRoles> ModulesWithRoles => Set<UserModuleRoles>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserModuleRoles> UserModuleRoles => Set<UserModuleRoles>();

        public DbSet<Collaborator> Collaborators => Set<Collaborator>();
        public DbSet<PersonalInformation> PersonalInformations => Set<PersonalInformation>();
        public DbSet<WorkingInformation> WorkingInformation => Set<WorkingInformation>();
        public DbSet<Vacation> Vacations => Set<Vacation>();
        public DbSet<PermitApplication> PermitApplications => Set<PermitApplication>();
        public DbSet<Catalog> Catalogs => Set<Catalog>();
        public DbSet<SubCatalog> SubCatalogs => Set<SubCatalog>();
        public DbSet<Salary> Salaries => Set<Salary>();

        public DbSet<Deduction> Deductions => Set<Deduction>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<OrdinaryPayroll> OrdinaryPayrolls => Set<OrdinaryPayroll>();
        public DbSet<WorkPositionHistory> WorkPositionHistories => Set<WorkPositionHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.HasPostgresExtension("uuid-ossp");

            #region Catalogos

            modelBuilder.HasPostgresEnum<CatalogType>("public","catalog_type_enum");            
            
            #endregion

            #region Creación de roles
            
            modelBuilder.HasPostgresEnum<RoleType>("public","role_type_enum");
            modelBuilder.HasPostgresEnum<PermissionType>("public","permission_type_enum");

            #endregion

            #region Crear un usuario

            modelBuilder.HasPostgresEnum<UserType>("public","user_type_enum");            
            modelBuilder.HasPostgresEnum<UserStatus>("public","user_status_enum");

            #endregion

            #region Registrar Colaborador enums

            modelBuilder.HasPostgresEnum<GenderType>("public","gender_type_enum");
            modelBuilder.HasPostgresEnum<IdentificationType>("public","identification_type_enum");
            modelBuilder.HasPostgresEnum<CollaboratorStatus>("public","collaborator_status_enum");
            modelBuilder.HasPostgresEnum<SalaryType>("public","salary_type_enum");
            modelBuilder.HasPostgresEnum<Currency>("public","currency_enum");
            modelBuilder.HasPostgresEnum<MaritalStatus>("public","marital_status_enum");

            #endregion
    

            #region Registro de permisos
    
            modelBuilder.HasPostgresEnum<PermitApplicationStatus>("public","permit_application_status_enum");
            modelBuilder.HasPostgresEnum<PermitApplicationType>("public","permit_application_type_enum");

            #endregion

            modelBuilder.HasPostgresEnum<DeductionType>("public","deduction_type_enum");
            modelBuilder.HasPostgresEnum<PayrollStatus>("public","payroll_status_enum");
            modelBuilder.HasPostgresEnum<PayrollType>("public","payroll_type_enum");
            modelBuilder.HasPostgresEnum<TaxType>("public","tax_type_enum");
            modelBuilder.HasPostgresEnum<SourceDeductionPayment>("public","source_deduction_payment_enum");
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc)
                    ));
                }
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                }
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}