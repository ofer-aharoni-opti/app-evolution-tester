using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Somo;

public static class UpdateSomo
{
    public sealed record Command(Guid Id, string Name, string Description) : IRequest<Response?>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed class Handler(ISomoRepository repository, IMapper mapper)
        : IRequestHandler<Command, Response?>
    {
        public async Task<Response?> Handle(Command request, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (model is null)
                return null;

            // State transition is owned by the domain model — UpdatedAt is set there.
            model.UpdateDetails(request.Name, request.Description);

            var saved = await repository.UpdateAsync(model, cancellationToken);
            if (!saved)
                return null;

            return mapper.Map<Response>(model);
        }
    }
}
