using System.Collections.Concurrent;
using Template.Application.Abstractions;
using Template.Domain.Models;
using Template.Infrastructure.Dto;

namespace Template.Infrastructure.Persistence;

// Adapter: implements the Application port.
// Internally stores DTOs — the domain model never leaks into persistence concerns.
// Swap this for a real DB implementation without changing Application or Domain.
public sealed class TestRepository : ITestRepository
{
    private readonly ConcurrentDictionary<string, TestDto> _store = new();

    public Task<TestModel?> GetByValueAsync(string value, CancellationToken ct = default)
    {
        if (!_store.TryGetValue(value, out var dto))
            return Task.FromResult<TestModel?>(null);

        // Map DTO → Domain: reconstruct the domain model from persisted state.
        var model = new TestModel { Value = dto.Value };
        if (dto.IsProcessed)
            model.MarkAsProcessed();

        return Task.FromResult<TestModel?>(model);
    }

    public Task AddAsync(TestModel model, CancellationToken ct = default)
    {
        // Map Domain → DTO: flatten the domain model for storage.
        var dto = new TestDto(model.Value, model.IsProcessed, model.ProcessedAt);
        _store[model.Value] = dto;

        return Task.CompletedTask;
    }
}
