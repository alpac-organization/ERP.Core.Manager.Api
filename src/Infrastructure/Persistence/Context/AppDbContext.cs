using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<UserProfile> Profiles => Set<UserProfile>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<UserModuleRoles> ModulesWithRoles => Set<UserModuleRoles>();  
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserModuleRoles> UserModuleRoles => Set<UserModuleRoles>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}