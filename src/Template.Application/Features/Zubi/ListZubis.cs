using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;

namespace Template.Application.Features.Zubi;

public static class ListZubis
{
    public sealed record Query : IRequest<Response>;

    public sealed record Item(
        Guid Id,
        string Name,
        string Description,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public sealed record Response(IReadOnlyCollection<Item> Items);

    private sealed class Handler(IZubiRepository repository, IMapper mapper)
        : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var models = await repository.GetAllAsync(cancellationToken);
            var items = models.Select(mapper.Map<Item>).ToArray();
            return new Response(items);
        }
    }
}
