using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class SalariesConfiguration : IEntityTypeConfiguration<Salary>
    {
        public void Configure(EntityTypeBuilder<Salary> builder)
        {
            builder.ToTable("salaries");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("salary_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

                        
            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            
            builder.Property(e => e.AmountInForeign)
                .HasColumnName("amount_in_foreign")
                .HasPrecision(18, 3) 
                .IsRequired();

            builder.Property(e => e.AmountInLocal)
                .HasColumnName("amount_in_local")
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(e => e.AmountSalary)
                .HasColumnName("amount_salary")
                .HasPrecision(18, 3)
                .IsRequired();

            builder.Property(e => e.SalaryType)
                .HasColumnName("salary_type")
                .HasColumnType("salary_type_enum")
                .IsRequired();

            builder.Property(e => e.BankSubCatalogId)
                .HasColumnName("bank_id")
                .IsRequired();

            builder.Property(e => e.Currency)
                .HasColumnName("currency")
                .HasColumnType("currency_enum")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date");


            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
       
            builder.HasOne(p => p.Collaborator)
                .WithMany(c => c.Salaries)
                .HasForeignKey(p => p.CollaboratorId) 
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasIndex(p => p.CollaboratorId)
                .IsUnique();
        }
    }
}