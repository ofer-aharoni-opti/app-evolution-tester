using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Zaba;

public static class GetZaba
{
    public sealed record Query(Guid Id) : IRequest<Response?>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private sealed class Handler(IZabaRepository repository, IMapper mapper)
        : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(request.Id, cancellationToken);

            // Null signals "not found" to the controller — let the API translate that into 404.
            return model is null ? null : mapper.Map<Response>(model);
        }
    }
}
