using Template.Domain.Models;

namespace Template.Application.Abstractions;

public interface ISumoRepository
{
    Task<SumoModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<SumoModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(SumoModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(SumoModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
