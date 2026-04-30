using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

// Adapter: implements the Application port.
// Internally stores DTOs — the domain model never leaks into persistence concerns.
// Swap this for a real DB implementation without changing Application or Domain.
public sealed class ZubiRepository : IZubiRepository
{
    private readonly ConcurrentDictionary<Guid, ZubiDto> _store = new();

    public Task<ZubiModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var dto))
            return Task.FromResult<ZubiModel?>(null);

        return Task.FromResult<ZubiModel?>(ToModel(dto));
    }

    public Task<IReadOnlyCollection<ZubiModel>> GetAllAsync(CancellationToken ct = default)
    {
        // Snapshot the values to avoid surfacing concurrent modifications mid-enumeration.
        var models = _store.Values.Select(ToModel).ToArray();
        return Task.FromResult<IReadOnlyCollection<ZubiModel>>(models);
    }

    public Task AddAsync(ZubiModel model, CancellationToken ct = default)
    {
        _store[model.Id] = ToDto(model);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(ZubiModel model, CancellationToken ct = default)
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

    private static ZubiDto ToDto(ZubiModel model) =>
        new(model.Id, model.Name, model.Description, model.CreatedAt, model.UpdatedAt);

    private static ZubiModel ToModel(ZubiDto dto) =>
        ZubiModel.Hydrate(dto.Id, dto.Name, dto.Description, dto.CreatedAt, dto.UpdatedAt);
}
