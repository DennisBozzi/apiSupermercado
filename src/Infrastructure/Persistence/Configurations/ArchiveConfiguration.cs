using ApiSupermercado.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiSupermercado.Infrastructure.Persistence.Configurations;

internal sealed class ArchiveConfiguration : IEntityTypeConfiguration<Archive>
{
    public void Configure(EntityTypeBuilder<Archive> b)
    {
        b.ToTable("archives");
        b.HasKey(a => a.Id);
        b.Property(a => a.Id).HasColumnName("id");
        b.Property(a => a.OwnerUserId).HasColumnName("owner_user_id");
        b.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(512).IsRequired();
        b.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(255).IsRequired();
        b.Property(a => a.SizeBytes).HasColumnName("size_bytes");
        b.Property(a => a.Bucket).HasColumnName("bucket").HasMaxLength(255).IsRequired();
        b.Property(a => a.StorageObject).HasColumnName("storage_object").HasMaxLength(1024).IsRequired();
        b.Property(a => a.Checksum).HasColumnName("checksum").HasMaxLength(128);
        b.Property(a => a.IsPublic).HasColumnName("is_public");
        b.Property(a => a.CreatedAt).HasColumnName("created_at");
        b.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(a => a.OwnerUserId);
        b.HasIndex(a => new { a.Bucket, a.StorageObject }).IsUnique();

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
