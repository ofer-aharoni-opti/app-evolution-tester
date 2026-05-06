using Template.Domain.Models;

namespace Template.Application.Abstractions;

// Port: defined in Application, implemented in Infrastructure.
// Application depends on the abstraction, not on persistence details.
public interface ISomoRepository
{
    Task<SomoModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<SomoModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(SomoModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(SomoModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
