using Microsoft.EntityFrameworkCore;
using ERP.Core.Database.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Core.Manager.Api.Infrastructure.Persistence.Configurations.Catalogs
{
    public class BranchesConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.ToTable("branches");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .HasColumnName("branch_id")
                .HasDefaultValueSql("gen_random_uuid()")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(e => e.BranchName)
                .HasColumnName("branch_name")
                .HasMaxLength(100);

            builder.Property(e => e.BranchAddress)
                .HasColumnName("branch_address")
                .HasMaxLength(500);

            builder.Property(e => e.PhoneNumber)
                .HasColumnName("phone_number");
                
            builder.Property(e => e.CompanyId)
                .HasColumnName("company_id")
                .IsRequired();

            builder.Property(e => e.CompanyAlias)
                .HasColumnName("company_alias")
                .IsRequired();

            builder.HasOne(c => c.Company)
                .WithMany(s => s.Branches)
                .HasForeignKey(s => s.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Payrolls)
                .WithOne(m => m.Branch)
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");         

            builder.HasIndex(e => e.CompanyId)
                .HasDatabaseName("IX_branches_company_id");   
        }
    }
}