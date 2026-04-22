using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class DeductionPaymentHistoriesConfiguration : IEntityTypeConfiguration<DeductionPaymentHistory>
    {
        public void Configure(EntityTypeBuilder<DeductionPaymentHistory> builder)
        {
            builder.ToTable("deductions_payment_histories");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("payment_history_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_payment_id");
            
            builder.Property(e => e.Origin)
                .HasColumnName("origin")
                .HasColumnType("source_deduction_payment_enum")
                .IsRequired();

            builder.Property(e => e.AmountPaid)
                .HasColumnName("amount_paid")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.DeductionId)
                .HasColumnName("deduction_id")
                .IsRequired();

            builder.Property(e => e.PaymentDate)
                .HasColumnName("payment_date")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Deduction)
                .WithMany(s => s.PaymentHistories)
                .HasForeignKey(s => s.DeductionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}