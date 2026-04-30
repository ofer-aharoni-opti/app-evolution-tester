using Template.Domain.Models;

namespace Template.Application.Abstractions;

// Port: defined in Application, implemented in Infrastructure.
// Application depends on the abstraction, not on persistence details.
public interface IZabaRepository
{
    Task<ZabaModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<ZabaModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ZabaModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(ZabaModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
