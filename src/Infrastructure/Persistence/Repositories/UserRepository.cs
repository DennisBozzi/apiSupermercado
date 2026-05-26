using ApiBozzis.Application.Abstractions.Repositories;
using ApiBozzis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiBozzis.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<bool> DocumentExistsAsync(string document, Guid? excludeUserId = null, CancellationToken ct = default)
        => excludeUserId is null
            ? _db.Users.AnyAsync(u => u.Document == document, ct)
            : _db.Users.AnyAsync(u => u.Document == document && u.Id != excludeUserId, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public void Update(User user) => _db.Users.Update(user);
}
