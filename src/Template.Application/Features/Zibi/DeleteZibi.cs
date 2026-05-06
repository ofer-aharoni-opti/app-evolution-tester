using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Zibi;

public static class DeleteZibi
{
    public sealed record Command(Guid Id) : IRequest<Response>;

    // Returning a flag (instead of Unit) lets the controller distinguish 204 vs 404.
    public sealed record Response(bool Deleted);

    private sealed class Handler(IZibiRepository repository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var deleted = await repository.DeleteAsync(request.Id, cancellationToken);
            return new Response(deleted);
        }
    }
}
