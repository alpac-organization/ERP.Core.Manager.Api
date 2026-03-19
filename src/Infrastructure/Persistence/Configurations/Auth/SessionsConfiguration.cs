using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Auth
{
    public class SessionsConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("sessions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("session_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.RefreshToken)
                .HasColumnName("refresh_token");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id");

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address");

            builder.Property(e => e.Device)
                .HasColumnName("device");

            builder.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasOne(u => u.User)
                .WithMany(p => p.Sessions)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Id)
                .HasDatabaseName("ix_session_id");
        }
    }
}
