using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Database.Domain.Entities.Payrolls;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class TypesIncomeConfiguration : IEntityTypeConfiguration<TypesIncome>
    {
        public void Configure(EntityTypeBuilder<TypesIncome> builder)
        {
            builder.ToTable("types_income");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("validity_deduction_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.IncomeTitle)
                .HasColumnName("income_title")
                .IsRequired();

            builder.Property(e => e.IncomeDescription)
                .HasColumnName("income_description")
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
        }
    }
}