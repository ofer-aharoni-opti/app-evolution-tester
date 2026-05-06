using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

public sealed class BananaRepository : IBananaRepository
{
    private readonly ConcurrentDictionary<Guid, BananaDto> _store = new();

    public Task<BananaModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(id, out var dto))
            return Task.FromResult<BananaModel?>(null);

        return Task.FromResult<BananaModel?>(ToModel(dto));
    }

    public Task<IReadOnlyCollection<BananaModel>> GetAllAsync(CancellationToken ct = default)
    {
        var models = _store.Values.Select(ToModel).ToArray();
        return Task.FromResult<IReadOnlyCollection<BananaModel>>(models);
    }

    public Task AddAsync(BananaModel model, CancellationToken ct = default)
    {
        _store[model.Id] = ToDto(model);
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(BananaModel model, CancellationToken ct = default)
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

    private static BananaDto ToDto(BananaModel model) =>
        new(model.Id, model.Name, model.Description, model.CreatedAt, model.UpdatedAt);

    private static BananaModel ToModel(BananaDto dto) =>
        BananaModel.Hydrate(dto.Id, dto.Name, dto.Description, dto.CreatedAt, dto.UpdatedAt);
}
