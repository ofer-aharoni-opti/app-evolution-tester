using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

// Adapter: implements the Application port.
// Internally stores DTOs — the domain model never leaks into persistence concerns.
// Swap this for a real DB implementation without changing Application or Domain.
public sealed class SomoRepository : ISomoRepository
{
    private readonly ConcurrentDictionary<Guid, SomoDto> _store = new();

    public Task<SomoModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var dto))
            return Task.FromResult<SomoModel?>(null);

        return Task.FromResult<SomoModel?>(ToModel(dto));
    }

    public Task<IReadOnlyCollection<SomoModel>> GetAllAsync(CancellationToken ct = default)
    {
        // Snapshot the values to avoid surfacing concurrent modifications mid-enumeration.
        var models = _store.Values.Select(ToModel).ToArray();
        return Task.FromResult<IReadOnlyCollection<SomoModel>>(models);
    }

    public Task AddAsync(SomoModel model, CancellationToken ct = default)
    {
        _store[model.Id] = ToDto(model);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(SomoModel model, CancellationToken ct = default)
    {
        // Only persist when the row already exists — keeps update/upsert semantics distinct.
        if (!_store.ContainsKey(model.Id))
            return Task.FromResult(false);

        _store[model.Id] = ToDto(model);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var removed = _store.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    private static SomoDto ToDto(SomoModel model) =>
        new(model.Id, model.Name, model.Description, model.CreatedAt, model.UpdatedAt);

    private static SomoModel ToModel(SomoDto dto) =>
        SomoModel.Hydrate(dto.Id, dto.Name, dto.Description, dto.CreatedAt, dto.UpdatedAt);
}
