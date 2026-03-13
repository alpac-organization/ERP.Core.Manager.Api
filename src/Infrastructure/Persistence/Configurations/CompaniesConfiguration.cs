using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations
{
    public class CompaniesConfiguration: IEntityTypeConfiguration<Companies>
    {
        public void Configure(EntityTypeBuilder<Companies> builder)
        {
            builder.ToTable("companies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("companie_id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(e => e.Alias)
                .HasColumnName("alias");    

            builder.Property(e => e.CompanieName)
                .HasColumnName("companie_name");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.ImageUrl)
                .HasColumnName("image_url");

            builder.Property(e => e.Code)
                .HasColumnName("code");

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasMany(c => c.Modules)
                .WithOne() // Un módulo pertenece a una sola empresa
                .HasForeignKey("companie_id") // Crea la columna en la tabla 'modules'
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}