using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class IrTaxTableConfiguration : IEntityTypeConfiguration<IrTaxTable>
    {
        public void Configure(EntityTypeBuilder<IrTaxTable> builder)
        {
            builder.ToTable("ir_tax_table");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("tax_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.BaseTax)
                .HasPrecision(18, 2)
                .HasColumnName("base_tax");

            builder.Property(e => e.FromAmount)
                .HasPrecision(18, 2)
                .HasColumnName("from_amount");

            builder.Property(e => e.ToAmount)
                .HasPrecision(18, 2)
                .HasColumnName("to_amount");

            builder.Property(e => e.Percentage)
                .IsRequired()
                .HasPrecision(18, 4)
                .HasColumnName("percentage");

            builder.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");

            builder.Property(e => e.StartDate)
                .IsRequired()
                .HasColumnName("start_date");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
        }
    }
}