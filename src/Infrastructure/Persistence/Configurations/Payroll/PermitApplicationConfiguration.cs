using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class PermitApplicationConfiguration : IEntityTypeConfiguration<PermitApplication>
    {
        public void Configure(EntityTypeBuilder<PermitApplication> builder)
        {
            builder.ToTable("permit_applications");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("permit_application_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("ix_permit_application_id");
            
            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("permit_application_status_enum")
                .IsRequired();

            builder.Property(e => e.Type)
                .HasColumnName("permit_application_type")
                .HasColumnType("permit_application_type_enum")
                .IsRequired();

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.StartTime)
                .HasColumnName("start_time")
                .IsRequired(false);

            builder.Property(e => e.EndTime)
                .HasColumnName("end_time")
                .IsRequired(false);

            builder.Property(e => e.ApprovedBy)
                .HasColumnName("approved_by");

            builder.Property(e => e.RejectedBy)
                .HasColumnName("rejected_by");

            builder.Property(e => e.RequestedBy)
                .HasColumnName("requested_by")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description");

            builder.Property(e => e.AmountDays)
                .HasColumnName("amount_days")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            builder.Property(e => e.CollaboratorCode)
                .HasColumnName("collaborator_code")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Collaborator)
                .WithMany(s => s.PermitApplications)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}