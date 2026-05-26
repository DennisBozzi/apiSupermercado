namespace ApiBozzis.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string FirebaseUid { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public bool EmailVerified { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Name { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Document { get; private set; }
    public DocumentType? DocumentType { get; private set; }
    public AuthProvider AuthProvider { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<int> _roles = [];
    public IReadOnlyList<int> Roles => _roles;

    public bool HasRole(int role) => _roles.Contains(role);
    public bool HasRole(ProfileType role) => _roles.Contains((int)role);
    public bool IsAdmin => HasRole(ProfileType.Admin);

    public void AddRole(int role)
    {
        if (_roles.Contains(role)) return;
        _roles.Add(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(int role)
    {
        if (!_roles.Remove(role)) return;
        UpdatedAt = DateTime.UtcNow;
    }

    private User() { }

    public static User Create(
        string firebaseUid,
        string email,
        bool emailVerified,
        AuthProvider provider,
        string? displayName,
        string? photoUrl)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = firebaseUid,
            Email = email.Trim().ToLowerInvariant(),
            EmailVerified = emailVerified,
            AuthProvider = provider,
            DisplayName = displayName,
            PhotoUrl = photoUrl,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = now,
            IsActive = true,
        };
        user._roles.Add((int)ProfileType.User);
        return user;
    }

    public void RegisterLogin(bool emailVerified, string? displayName, string? photoUrl)
    {
        var now = DateTime.UtcNow;
        EmailVerified = emailVerified;
        if (!string.IsNullOrWhiteSpace(displayName)) DisplayName = displayName;
        if (!string.IsNullOrWhiteSpace(photoUrl)) PhotoUrl = photoUrl;
        LastLoginAt = now;
        UpdatedAt = now;
    }

    public void LinkProvider(string firebaseUid, AuthProvider provider)
    {
        var changed = false;
        if (FirebaseUid != firebaseUid) { FirebaseUid = firebaseUid; changed = true; }
        if (AuthProvider != provider) { AuthProvider = provider; changed = true; }
        if (changed) UpdatedAt = DateTime.UtcNow;
    }

    public void SetDocument(string document, DocumentType type)
    {
        Document = document;
        DocumentType = type;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string? displayName, string? photoUrl)
    {
        DisplayName = displayName;
        PhotoUrl = photoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(string? name, DateOnly? birthDate)
    {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        BirthDate = birthDate;
        if (!string.IsNullOrWhiteSpace(Name)) DisplayName = Name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
