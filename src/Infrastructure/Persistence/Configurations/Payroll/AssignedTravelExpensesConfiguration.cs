

using ERP.Core.Database.Domain.Entities.Payrolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class AssignedTravelExpensesConfiguration : IEntityTypeConfiguration<AssignedTravelExpenses>
    {
        public void Configure(EntityTypeBuilder<AssignedTravelExpenses> builder)
        {
            builder.ToTable("assigned_travel_expenses");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("assigned_travel_expense_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.AmountInDollars)
                .HasColumnName("amount_in_dollars")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.AmountInLocalCurrency)
                .HasColumnName("amount_in_local_currency")
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(e => e.Currency)
                .HasColumnName("currency")
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

            builder.HasOne(d => d.TypeIncome)
                .WithMany()
                .HasForeignKey(d => d.TypeIncomeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}