using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Auth
{
    public class UserModuleRolesConfiguration : IEntityTypeConfiguration<UserModuleRoles>
    {
        public void Configure(EntityTypeBuilder<UserModuleRoles> builder)
        {
            builder.ToTable("user_module_roles");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("user_module_role_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.ModuleCode)
                .HasColumnName("module_code")
                .IsRequired();

            builder.Property(e => e.UserProfileId)
                .HasColumnName("user_profile_id")
                .IsRequired();

            builder.Property(e => e.RoleId)
                .HasColumnName("role_id")
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(u => u.UserProfile)
                .WithMany(p => p.UserModuleRole)
                .HasForeignKey(p => p.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.UserProfileId, e.ModuleCode })
                .IsUnique()
                .HasDatabaseName("IX_Unique_User_Module_Role");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);
        }
    }
}