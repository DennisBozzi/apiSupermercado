using ApiBozzis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiBozzis.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.FirebaseUid).HasColumnName("firebase_uid").HasMaxLength(128).IsRequired();
        b.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        b.Property(u => u.EmailVerified).HasColumnName("email_verified");
        b.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(120);
        b.Property(u => u.Name).HasColumnName("name").HasMaxLength(120);
        b.Property(u => u.BirthDate).HasColumnName("birth_date");
        b.Property(u => u.PhotoUrl).HasColumnName("photo_url").HasMaxLength(2048);
        b.Property(u => u.Document).HasColumnName("document").HasMaxLength(14);
        b.Property(u => u.DocumentType).HasColumnName("document_type").HasConversion<int?>();
        b.Property(u => u.AuthProvider).HasColumnName("auth_provider").HasConversion<int>();
        b.Property(u => u.CreatedAt).HasColumnName("created_at");
        b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        b.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
        b.Property(u => u.IsActive).HasColumnName("is_active");
        b.PrimitiveCollection<List<int>>("_roles")
            .HasColumnName("roles")
            .HasColumnType("integer[]")
            .IsRequired();
        b.Ignore(u => u.Roles);

        b.HasIndex(u => u.FirebaseUid).IsUnique();
        b.HasIndex(u => u.Email).IsUnique();
        b.HasIndex(u => u.Document).IsUnique().HasFilter("document IS NOT NULL");
    }
}
