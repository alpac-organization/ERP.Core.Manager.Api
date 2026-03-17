using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Auth
{
    public class RolesConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("role_id")
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(e => e.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(180)
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description");

            builder.Property(e => e.RoleType)
                .HasColumnName("role_type");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasMany(u => u.Permissions)
                .WithOne(p => p.Role)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Id)
                .HasDatabaseName("ix_role_id");
        }
    }
}