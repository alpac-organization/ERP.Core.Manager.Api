using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class VacationsConfiguration : IEntityTypeConfiguration<Vacation>
    {
        public void Configure(EntityTypeBuilder<Vacation> builder)
        {
            builder.ToTable("vacations");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("vacation_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.AvailableVacations)
                .HasColumnName("available_vacations")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(e => e.EnjoyedVacation)
                .HasColumnName("enjoyed_vacation")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(e => e.GeneredVacation)
                .HasColumnName("genered_vacation")
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
       
            builder.HasOne(p => p.Collaborator)
                .WithOne(c => c.Vacation)
                .HasForeignKey<Vacation>(p => p.CollaboratorId) 
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasIndex(p => p.CollaboratorId)
                .IsUnique();
        }
    }
}