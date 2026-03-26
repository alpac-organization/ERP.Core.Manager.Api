using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

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
        public DbSet<VacationRequest> VacationRequests => Set<VacationRequest>();
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<SubCatalog> SubCatalogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}