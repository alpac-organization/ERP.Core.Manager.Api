using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Payroll;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Payroll
{
    public class PersonalInformationConfiguration : IEntityTypeConfiguration<PersonalInformation>
    {
        public void Configure(EntityTypeBuilder<PersonalInformation> builder)
        {
            builder.ToTable("personal_informations");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("personal_information_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.Address)
                .HasColumnName("address")
                .IsRequired(false);

            builder.Property(e => e.PersonalEmail)
                .HasColumnName("personal_email")
                .IsRequired(false);
            
            builder.Property(e => e.PersonalPhoneNumber)
                .HasColumnName("personal_phone_number")
                .IsRequired(false);

            builder.Property(e => e.CollaboratorId)
                .HasColumnName("collaborator_id")
                .IsRequired();

            builder.Property(e => e.Departament)
                .HasColumnName("departament")
                .IsRequired(false);

            builder.Property(e => e.Birthdate)
                .HasColumnName("birthdate")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
       
            builder.HasOne(p => p.Collaborator)
                .WithOne(c => c.PersonalInformation)
                .HasForeignKey<PersonalInformation>(p => p.CollaboratorId) 
                .OnDelete(DeleteBehavior.Cascade); 

            builder.HasIndex(p => p.CollaboratorId)
                .IsUnique();
        }
    }
}