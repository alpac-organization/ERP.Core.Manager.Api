using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
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
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.ModuleName)
                .HasColumnName("module_name")
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .IsRequired();

            builder.Property(e => e.PathRedirect)
                .HasColumnName("path_redirect")
                .HasDefaultValue("/dashboard")
                .IsRequired();  

            builder.Property(e => e.ImageUrl)
                .HasColumnName("image_url")
                .IsRequired(false);

            builder.Property(e => e.Description)
                .HasColumnName("description");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasIndex(e => e.Code)
                .HasDatabaseName("ix_modules_company_code");
        }
    }
}