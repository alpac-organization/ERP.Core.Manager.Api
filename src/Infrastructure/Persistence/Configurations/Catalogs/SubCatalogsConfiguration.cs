using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    /// <summary>
    /// Configuración de la entidad SubCatalog para el mapeo con la base de datos.
    /// </summary>
    public class SubCatalogsConfiguration : IEntityTypeConfiguration<SubCatalog>
    {
        public void Configure(EntityTypeBuilder<SubCatalog> builder)
        {
            builder.ToTable("sub_catalogs");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("sub_catalog_id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.CatalogName)
                .HasColumnName("catalog_name")
                .HasMaxLength(150);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.CatalogId)
                .HasColumnName("catalog_id")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(e => e.Catalog)
                .WithMany(c => c.SubCatalogs)
                .HasForeignKey(e => e.CatalogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}