using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations
{
    public class ModulesConfiguration: IEntityTypeConfiguration<Modules>
    {
        public void Configure(EntityTypeBuilder<Modules> builder)
        {
            builder.ToTable("modules");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("module_id")
                .HasDefaultValueSql("gen_random_uuid()");
 
            builder.Property(e => e.ModuleName)
                .HasColumnName("module_name");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                
        }
    }
}