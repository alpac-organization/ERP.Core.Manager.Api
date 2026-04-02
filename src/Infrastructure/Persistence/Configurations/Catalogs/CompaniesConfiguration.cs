using Microsoft.EntityFrameworkCore;
using ERP.Core.Manager.Api.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class CompaniesConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("companies");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("company_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.Alias)
                .HasColumnName("alias")
                .HasMaxLength(100);

            builder.Property(e => e.CompanieName)
                .HasColumnName("company_name")
                .HasMaxLength(200);

            builder.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(50);

            builder.Property(e => e.ImageUrl)
                .HasColumnName("image_url");

            builder.Property(e => e.NeutralImageUrl)
                .HasColumnName("neutral_image_url")
                .HasDefaultValue(null);

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasMany(c => c.Catalogs)
                .WithOne(m => m.Company)
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Collaborators)
                .WithOne(m => m.Company)
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("IX_companies_code");

            builder.HasIndex(e => e.Id)
                .IsUnique()
                .HasDatabaseName("IX_companies_id");
        }
    }
}