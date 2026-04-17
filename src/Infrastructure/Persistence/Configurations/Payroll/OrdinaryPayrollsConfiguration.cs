using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class OrdinaryPayrollsConfiguration : IEntityTypeConfiguration<OrdinaryPayroll>
    {
        public void Configure(EntityTypeBuilder<OrdinaryPayroll> builder)
        {
            builder.ToTable("ordinary_payrolls");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("ordinary_payroll_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_ordinary_payroll_id");

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.Inss)
                .HasColumnName("inss")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Ir)
                .HasColumnName("ir")
                .HasPrecision(18, 2)
                .IsRequired();
            
            builder.Property(e => e.TotalDeducctions)
                .HasColumnName("total_deductions")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Deductions)
                .HasColumnName("deductions")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.GrossSalary)
                .HasColumnName("gross_salary")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Vacations)
                .HasColumnName("vacations")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Overtime)
                .HasColumnName("overtimes")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Bonus)
                .HasColumnName("bonus")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.TotalToPay)
                .HasColumnName("total_to_pay")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.PayrollId)
                .HasColumnName("payroll_id")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Collaborator)
                .WithMany(s => s.OrdinaryPayrolls)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);    
        }
    }
}