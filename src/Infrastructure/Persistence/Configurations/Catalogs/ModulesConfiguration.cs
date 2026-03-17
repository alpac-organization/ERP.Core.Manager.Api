using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class ModulesConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.ToTable("modules");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("module_id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.ModuleName)
                .HasColumnName("module_name")
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(e => e.CompanyId)
                .HasColumnName("company_id");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasIndex(e => e.CompanyId)
                .HasDatabaseName("ix_modules_company_id");
        }
    }
}