using Template.Domain.Models;

namespace Template.Application.Abstractions;

// Port: defined in Application, implemented in Infrastructure.
// Application depends on the abstraction, not on persistence details.
public interface IZubiRepository
{
    Task<ZubiModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<ZubiModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ZubiModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(ZubiModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
