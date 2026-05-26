using ApiBozzis.Domain.Entities;

namespace ApiBozzis.Application.Abstractions.Repositories;

public interface IArchiveRepository
{
    Task<Archive?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Archive>> ListByOwnerAsync(Guid ownerUserId, int skip, int take, CancellationToken ct = default);
    Task AddAsync(Archive archive, CancellationToken ct = default);
    void Remove(Archive archive);
}
