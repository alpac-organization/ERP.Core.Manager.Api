using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class PayrollsConfiguration : IEntityTypeConfiguration<Core.Database.Domain.Entities.Payrolls.Payroll>
    {
        public void Configure(EntityTypeBuilder<Core.Database.Domain.Entities.Payrolls.Payroll> builder)
        {
            builder.ToTable("payrolls");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("payroll_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_payroll_id");

            builder.Property(e => e.TotalToPay)
                .HasColumnName("total_to_pay")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("payroll_status")
                .HasColumnType("payroll_status_enum")
                .IsRequired();

            builder.Property(e => e.PayrollType)
                .HasColumnName("payroll_type")
                .HasColumnType("payroll_type_enum");

            builder.Property(e => e.CompanyId)
                .HasColumnName("company_id")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .IsRequired(false);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Company)
                .WithMany(s => s.Payrolls)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);    

            builder.HasMany(c => c.OrdinaryPayrolls)
                .WithOne(s => s.Payroll)
                .HasForeignKey(s => s.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);        
        }
    }
}