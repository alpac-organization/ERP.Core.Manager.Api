using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class IncomeTaxAccrualConfiguration : IEntityTypeConfiguration<IncomeTaxAccrual>
    {
        public void Configure(EntityTypeBuilder<IncomeTaxAccrual> builder)
        {
            builder.ToTable("income_tax_accrual");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("income_tax_accrual_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_income_tax_id");

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.PayrollId)
                .HasColumnName("payroll_id ")
                .IsRequired();

            builder.Property(e => e.NumberOfFortnights)
                .HasColumnName("number_of_fortnights")
                .IsRequired();

            builder.Property(e => e.SalaryEarned)
                .HasPrecision(18,0)
                .HasColumnName("salary_earned")
                .IsRequired();

            builder.Property(e => e.AccumulatedIR)
                .HasPrecision(18,0)
                .HasColumnName("accumulated_ir")
                .IsRequired();

            builder.Property(e => e.RegisterDate)
                .HasColumnName("register_date")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Collaborator)
                .WithMany(s => s.IncomeTaxAccruals)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Payroll)
                .WithMany()
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}