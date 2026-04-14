using ERP.Core.Database.Domain.Entities.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class CollaboratorsConfiguration : IEntityTypeConfiguration<Collaborator>
    {
        public void Configure(EntityTypeBuilder<Collaborator> builder)
        {
            builder.ToTable("collaborators");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("collaborator_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("IX_collaborator_id");

            builder.Property(e => e.PictureUrl)
                .HasColumnName("picture_url")
                .HasDefaultValue(null)
                .IsRequired(false);

            builder.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .IsRequired();

            builder.Property(e => e.SecondName)
                .HasColumnName("second_name")
                .IsRequired(false);
            
            builder.Property(e => e.ThirdName)
                .HasColumnName("third_name")
                .IsRequired(false);

            builder.Property(e => e.FirstLastname)
                .HasColumnName("first_lastname")
                .IsRequired();

            builder.Property(e => e.SecondLastname)
                .HasColumnName("second_lastname")
                .IsRequired(false);

            builder.Property(e => e.IdentificationNumber)
                .HasColumnName("identification_number")
                .IsRequired();

            builder.HasIndex(e => e.IdentificationNumber)
                .HasDatabaseName("IX_collaborators_identification_number");

            builder.Property(e => e.IdentificationType)
                .HasColumnName("identification_type")
                .HasColumnType("identification_type_enum")
                .IsRequired();

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("collaborator_status_enum")
                .IsRequired();

            builder.Property(e => e.Gender)
                .HasColumnName("gender")
                .HasColumnType("gender_type_enum")
                .IsRequired();

            builder.Property(e => e.CompanyId)
                .HasColumnName("company_id")
                .IsRequired();

            builder.Property(e => e.RegisteredBy)
                .HasColumnName("registered_by")
                .IsRequired();

            builder.Property(e => e.CollaboratorCode)
                .HasColumnName("collaborator_code")
                .IsRequired();

            builder.HasIndex(e => e.CollaboratorCode)
                .IsUnique()
                .HasDatabaseName("IX_collaborators_collaborator_code");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(c => c.Company)
                .WithMany(s => s.Collaborators)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            //Relacionar la información personal.
            builder.HasOne(c => c.PersonalInformation)
                .WithOne(s => s.Collaborator)
                .HasForeignKey<PersonalInformation>(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade);

            //Relacionar la informacion de trabajo.
            builder.HasOne(c => c.WorkingInformation)
                .WithOne(s => s.Collaborator)
                .HasForeignKey<WorkingInformation>(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade); 

            //Relacionar el control de vacaciones.
            builder.HasOne(c => c.Vacation)
                .WithOne(s => s.Collaborator)
                .HasForeignKey<Vacation>(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade); 

            //Registrar el control de solicitudes de vacaciones
            builder.HasMany(c => c.PermitApplications)
                .WithOne(s => s.Collaborator)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade);

            //Registrar el control de salarios
            builder.HasMany(c => c.Salaries)
                .WithOne(s => s.Collaborator)
                .HasForeignKey(s => s.CollaboratorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}