using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Enums;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;


namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
    {
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Module> Modules => Set<Module>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            // Registrar Enums
            modelBuilder.HasPostgresEnum<CatalogType>();
            modelBuilder.HasPostgresEnum<UserStatus>();
            modelBuilder.HasPostgresEnum<RoleType>();
            modelBuilder.HasPostgresEnum<PermissionType>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}