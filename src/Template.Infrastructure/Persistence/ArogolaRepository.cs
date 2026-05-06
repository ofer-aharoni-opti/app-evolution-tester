using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

public sealed class ArogolaRepository : IArogolaRepository
{
    private readonly ConcurrentDictionary<Guid, ArogolaDto> _store = new();

    public Task<ArogolaModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var dto))
            return Task.FromResult<ArogolaModel?>(null);

        return Task.FromResult<ArogolaModel?>(ToModel(dto));
    }

    public Task<IReadOnlyCollection<ArogolaModel>> GetAllAsync(CancellationToken ct = default)
    {
        var models = _store.Values.Select(ToModel).ToArray();
        return Task.FromResult<IReadOnlyCollection<ArogolaModel>>(models);
    }

    public Task AddAsync(ArogolaModel model, CancellationToken ct = default)
    {
        _store[model.Id] = ToDto(model);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(ArogolaModel model, CancellationToken ct = default)
    {
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

    private static ArogolaDto ToDto(ArogolaModel model) =>
        new(model.Id, model.Name, model.Description, model.CreatedAt, model.UpdatedAt);

    private static ArogolaModel ToModel(ArogolaDto dto) =>
        ArogolaModel.Hydrate(dto.Id, dto.Name, dto.Description, dto.CreatedAt, dto.UpdatedAt);
}
