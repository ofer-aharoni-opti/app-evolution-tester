using System.Reflection;
using Template.Application.Features.Test;
using Template.Infrastructure.Persistence;

namespace Template.Application.UnitTests.Features.Test;

public sealed class CreateTestTests
{
    [Fact]
    public async Task Handle_ReturnsHandledMessage()
    {
        var repository = new TestRepository();

        var handlerType = typeof(CreateTest).GetNestedType("Handler", BindingFlags.NonPublic);
        Assert.NotNull(handlerType);

        var handlerObj = Activator.CreateInstance(handlerType!, [repository, TestInitializer.Mapper]);
        Assert.NotNull(handlerObj);

        var handler = Assert.IsAssignableFrom<MediatR.IRequestHandler<CreateTest.Command, CreateTest.Response>>(handlerObj);

        var response = await handler.Handle(new CreateTest.Command("abc"), CancellationToken.None);

        Assert.Equal("handled:abc", response.Message);
        Assert.True(response.IsProcessed);
        Assert.NotNull(response.ProcessedAt);
    }
}
