using ApiBozzis.Domain.Entities;

namespace ApiBozzis.Application.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    bool EmailVerified,
    string? DisplayName,
    string? Name,
    DateOnly? BirthDate,
    string? PhotoUrl,
    string? Document,
    DocumentType? DocumentType,
    AuthProvider AuthProvider,
    IReadOnlyList<int> Roles,
    DateTime CreatedAt);

public sealed record UpdateProfileRequest(
    string? Name,
    DateOnly? BirthDate,
    string? PhotoUrl);

public sealed record SetDocumentRequest(string Document, DocumentType DocumentType);
public sealed record UpdatePhotoRequest(Stream Content, string FileName, string ContentType, long SizeBytes);

internal static class UserMappings
{
    public static UserResponse ToResponse(this User u) => new(
        u.Id, u.Email, u.EmailVerified, u.DisplayName, u.Name, u.BirthDate,
        u.PhotoUrl, u.Document, u.DocumentType, u.AuthProvider, u.Roles, u.CreatedAt);
}
