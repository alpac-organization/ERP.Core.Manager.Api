using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Auth
{
    public class PermissionsConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("permissions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("permission_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.PermissionName)
                .HasColumnName("permission_name");

            builder.Property(e => e.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(400);

            builder.Property(e => e.PermissionType)
                .HasColumnName("permission_type")
                .HasConversion<string>()
                .IsRequired();
            
            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");
            
            builder.HasOne(p => p.Role)
                .WithMany(u => u.Permissions)
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Id)
                .HasDatabaseName("ix_permission_id");
        }
    }
}