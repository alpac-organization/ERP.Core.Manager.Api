using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class ValidityDeductionsConfiguration : IEntityTypeConfiguration<ValidityDeductions>
    {
        public void Configure(EntityTypeBuilder<ValidityDeductions> builder)
        {
            builder.ToTable("validity_deductions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("validity_deduction_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.StartDate)
                .IsRequired()
                .HasColumnName("start_date");

            builder.Property(e => e.EndDate)
                .IsRequired(false)
                .HasColumnName("end_date");

            builder.Property(e => e.TitleTax)
                .IsRequired(false)
                .HasColumnName("title_tax");

            builder.Property(e => e.Description)
                .IsRequired(false)
                .HasColumnName("description");

            builder.Property(e => e.Value)
                .IsRequired()
                .HasPrecision(18, 4)
                .HasColumnName("value");

            builder.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
        }
    }
}