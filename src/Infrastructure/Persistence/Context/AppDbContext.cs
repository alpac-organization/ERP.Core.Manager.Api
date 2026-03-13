using Microsoft.EntityFrameworkCore;

using ERP.Core.Manager.Api.Domain.Entities;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
    {
        public DbSet<Companies> Companies => Set<Companies>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}