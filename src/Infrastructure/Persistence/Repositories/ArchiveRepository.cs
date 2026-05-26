using ApiSupermercado.Application.Abstractions.Repositories;
using ApiSupermercado.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiSupermercado.Infrastructure.Persistence.Repositories;

internal sealed class ArchiveRepository : IArchiveRepository
{
    private readonly AppDbContext _db;
    public ArchiveRepository(AppDbContext db) => _db = db;

    public Task<Archive?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Archives.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Archive>> ListByOwnerAsync(Guid ownerUserId, int skip, int take, CancellationToken ct = default)
        => await _db.Archives
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

    public async Task AddAsync(Archive archive, CancellationToken ct = default)
        => await _db.Archives.AddAsync(archive, ct);

    public void Remove(Archive archive) => _db.Archives.Remove(archive);
}
