using ERP.Core.Manager.Api.Domain.Entities.Authentication;
using ERP.Core.Manager.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Auth
{
    public class UsersConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("user_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.UserName)
                .HasColumnName("user_name")
                .IsRequired();

            builder.Property(e => e.Email)
                .HasColumnName("email");
            
            builder.Property(e => e.UserStatus)
                .HasConversion<string>(
                v => v.ToString(), // Al guardar: convierte enum a string
                v => (UserStatus) Enum.Parse(typeof(UserStatus), v));

            builder.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.HasMany(u => u.Profiles)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Sessions)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}