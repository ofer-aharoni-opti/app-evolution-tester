using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;
using Template.Domain.Models;

namespace Template.Application.Features.Test;

public static class CreateTest
{
    public sealed record Command(string Value) : IRequest<Response>;

    // Application Response carries all data the use case produces.
    // This may differ from what the API exposes — the WebApi layer
    // can introduce its own response contract to reshape the output.
    public sealed record Response(string Message, bool IsProcessed, DateTime? ProcessedAt);

    private sealed class Handler(ITestRepository repository, IMapper mapper)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var model = new TestModel { Value = request.Value };

            // Business logic is invoked on the domain model, not implemented in the handler.
            model.MarkAsProcessed();

            // Persistence is accessed through the port (ITestRepository),
            // not through infrastructure details.
            await repository.AddAsync(model, cancellationToken);

            return mapper.Map<Response>(model);
        }
    }
}
