using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations
{
    public class CatalogsConfiguration : IEntityTypeConfiguration<Catalog>
    {
        public void Configure(EntityTypeBuilder<Catalog> builder)
        {
            builder.ToTable("catalogs");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("catalog_id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.CatalogName)
                .HasColumnName("catalog_name")
                .HasMaxLength(150);

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(500);

            builder.Property(e => e.CatalogType)
                .HasColumnName("catalog_type")
                .HasColumnType("catalog_type")
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.HasMany(c => c.SubCatalogs)
                .WithOne(s => s.Catalog)
                .HasForeignKey(s => s.CatalogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}