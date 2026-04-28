using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Enums;
using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class IncomeConfiguration : IEntityTypeConfiguration<Income>
    {
        public void Configure(EntityTypeBuilder<Income> builder)
        {
            builder.ToTable("incomes");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("income_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_income_id");

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.AmountInDollars)
                .HasColumnName("amount_in_dollars")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.AmountInLocal)
                .HasColumnName("amount_in_local")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsRequired();

            builder.Property(e => e.Currency)
                .HasColumnName("currency")
                .IsRequired();

            builder.Property(e => e.PayrollId)
                .HasColumnName("payroll_id ")
                .IsRequired();
            
            builder.Property(e => e.IncomeTypeId)
                .HasColumnName("income_type_id")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(d => d.TypesIncome)
                .WithMany()
                .HasForeignKey(d => d.IncomeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Payroll)
                .WithMany()
                .HasForeignKey(d => d.PayrollId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Collaborator)
                .WithMany(s => s.Incomes)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}