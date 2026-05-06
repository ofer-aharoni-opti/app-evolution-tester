using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Banana;

public static class GetBanana
{
    public sealed record Query(Guid Id) : IRequest<Response?>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    private sealed class Handler(IBananaRepository repository, IMapper mapper)
        : IRequestHandler<Query, Response?>
    {
        public async Task<Response?> Handle(Query request, CancellationToken cancellationToken)
        {
            var model = await repository.GetByIdAsync(request.Id, cancellationToken);
            return model is null ? null : mapper.Map<Response>(model);
        }
    }
}
