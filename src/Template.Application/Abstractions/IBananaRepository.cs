using Template.Domain.Models;

namespace Template.Application.Abstractions;

public interface IBananaRepository
{
    Task<BananaModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyCollection<BananaModel>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(BananaModel model, CancellationToken ct = default);
    Task<bool> UpdateAsync(BananaModel model, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
