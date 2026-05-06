using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;
using Template.Domain.Models;

namespace Template.Application.Features.Somo;

public static class CreateSomo
{
    public sealed record Command(string Name, string Description) : IRequest<Response>;

    // Application Response matches the API shape exactly —
    // no WebApi response contract needed, the controller returns this directly.
    public sealed record Response(Guid Id, string Name, string Description, DateTime CreatedAt);

    private sealed class Handler(ISomoRepository repository, IMapper mapper)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            // Construction invariants live in the domain factory, not the handler.
            var model = SomoModel.Create(request.Name, request.Description);

            await repository.AddAsync(model, cancellationToken);

            return mapper.Map<Response>(model);
        }
    }
}
