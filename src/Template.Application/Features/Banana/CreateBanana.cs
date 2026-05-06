using MapsterMapper;
using MediatR;
using Template.Application.Abstractions;
using Template.Domain.Models;

namespace Template.Application.Features.Banana;

public static class CreateBanana
{
    public sealed record Command(string Name, string Description) : IRequest<Response>;

    public sealed record Response(Guid Id, string Name, string Description, DateTime CreatedAt);

    private sealed class Handler(IBananaRepository repository, IMapper mapper)
        : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var model = BananaModel.Create(request.Name, request.Description);

            await repository.AddAsync(model, cancellationToken);

            return mapper.Map<Response>(model);
        }
    }
}
