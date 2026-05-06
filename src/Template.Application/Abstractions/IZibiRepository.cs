using Template.Domain.Models;

namespace Template.Application.Abstractions;

// Port: defined in Application, implemented in Infrastructure.
// Application depends on the abstraction, not on persistence details.
public interface IZibiRepository
{
    Task<ZibiModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<ZibiModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ZibiModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(ZibiModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
