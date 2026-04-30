using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;
using Template.Domain.Models;

namespace Template.Application.Features.Test;

public static class GetTest
{
    public sealed record Query(string Value) : IRequest<Response>;

    // Application Response matches the API shape exactly —
    // no WebApi response contract needed, the controller returns this directly.
    public sealed record Response(string Echo);

    private sealed class Handler(ITestRepository repository, IMapper mapper)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var model = await repository.GetByValueAsync(request.Value, cancellationToken);

            // Fall back to a new model when nothing is persisted yet.
            model ??= new TestModel { Value = request.Value };

            return mapper.Map<Response>(model);
        }
    }
}
