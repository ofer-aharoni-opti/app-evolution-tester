using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

public sealed class SumoRepository : ISumoRepository
{
    private readonly ConcurrentDictionary<Guid, SumoDto> _store = new();

    public Task<SumoModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var dto))
            return Task.FromResult<SumoModel?>(null);

        return Task.FromResult<SumoModel?>(ToModel(dto));
    }

    public Task<IReadOnlyCollection<SumoModel>> GetAllAsync(CancellationToken ct = default)
    {
        var models = _store.Values.Select(ToModel).ToArray();
        return Task.FromResult<IReadOnlyCollection<SumoModel>>(models);
    }

    public Task AddAsync(SumoModel model, CancellationToken ct = default)
    {
        _store[model.Id] = ToDto(model);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(SumoModel model, CancellationToken ct = default)
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

    private static SumoDto ToDto(SumoModel model) =>
        new(model.Id, model.Name, model.Description, model.CreatedAt, model.UpdatedAt);

    private static SumoModel ToModel(SumoDto dto) =>
        SumoModel.Hydrate(dto.Id, dto.Name, dto.Description, dto.CreatedAt, dto.UpdatedAt);
}
