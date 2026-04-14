using ERP.Core.Database.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
                .HasColumnName("refresh_token")
                .IsRequired();

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Device)
                .HasColumnName("device")
                .ValueGeneratedNever();

            builder.Property(e => e.CompanyCode)
                .HasColumnName("company_code")
                .IsRequired();

            builder.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();

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
