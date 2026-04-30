using Template.Domain.Models;

namespace Template.Application.Abstractions;

// Port: defined in Application, implemented in Infrastructure.
// Application depends on the abstraction, not on persistence details.
public interface ITestRepository
{
    Task<TestModel?> GetByValueAsync(string value, CancellationToken ct = default);
    Task AddAsync(TestModel model, CancellationToken ct = default);
}
