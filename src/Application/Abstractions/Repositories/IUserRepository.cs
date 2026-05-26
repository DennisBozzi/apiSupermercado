using ApiSupermercado.Domain.Entities;

namespace ApiSupermercado.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> DocumentExistsAsync(string document, Guid? excludeUserId = null, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
}
