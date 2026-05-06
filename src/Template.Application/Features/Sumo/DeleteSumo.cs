using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Sumo;

public static class DeleteSumo
{
    public sealed record Command(Guid Id) : IRequest<Response>;

    public sealed record Response(bool Deleted);

    private sealed class Handler(ISumoRepository repository)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var deleted = await repository.DeleteAsync(request.Id, cancellationToken);
            return new Response(deleted);
        }
    }
}
