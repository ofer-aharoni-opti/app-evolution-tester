using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Arogola;

public static class UpdateArogola
{
    public sealed record Command(Guid Id, string Name, string Description) : IRequest<Response?>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime UpdatedAt);

    private sealed class Handler(IArogolaRepository repository, IMapper mapper)
        : IRequestHandler<Command, Response?>
    {
        public async Task<Response?> Handle(Command request, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (model is null)
                return null;

            model.UpdateDetails(request.Name, request.Description);

            var saved = await repository.UpdateAsync(model, cancellationToken);
            if (!saved)
                return null;

            return mapper.Map<Response>(model);
        }
    }
}
