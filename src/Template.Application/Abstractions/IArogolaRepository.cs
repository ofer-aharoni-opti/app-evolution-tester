using Template.Domain.Models;

namespace Template.Application.Abstractions;

public interface IArogolaRepository
{
    Task<ArogolaModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<ArogolaModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(ArogolaModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(ArogolaModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
