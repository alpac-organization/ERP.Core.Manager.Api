using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class AssignedTravelExpensesHistoryConfiguration : IEntityTypeConfiguration<AssignedTravelExpensesHistory>
    {
        public void Configure(EntityTypeBuilder<AssignedTravelExpensesHistory> builder)
        {
            builder.ToTable("assigned_travel_expenses_histories");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("assigned_travel_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.PayrollId)
                .HasColumnName("payroll_id")
                .IsRequired();

            builder.Property(e => e.Feeding)
                .HasColumnName("feeding")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Lodging)
                .HasColumnName("lodging")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Transport)
                .HasColumnName("transport")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.TotalAmountPaid)
                .HasColumnName("total_amount_paid")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(d => d.Collaborator)
                .WithMany()
                .HasForeignKey(d => d.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Payroll)
                .WithMany(s => s.AssignedTravelExpensesHistories)
                .HasForeignKey(s => s.PayrollId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}