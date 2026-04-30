using System.Reflection;
using Template.Application.Features.Test;
using Template.Infrastructure.Persistence;

namespace Template.Application.UnitTests.Features.Test;

public sealed class GetTestTests
{
    [Fact]
    public async Task Handle_ReturnsEcho()
    {
        var repository = new TestRepository();

        var handlerType = typeof(GetTest).GetNestedType("Handler", BindingFlags.NonPublic);
        Assert.NotNull(handlerType);

        var handlerObj = Activator.CreateInstance(handlerType!, [repository, TestInitializer.Mapper]);
        Assert.NotNull(handlerObj);

        var handler = Assert.IsAssignableFrom<MediatR.IRequestHandler<GetTest.Query, GetTest.Response>>(handlerObj);

        var response = await handler.Handle(new GetTest.Query("abc"), CancellationToken.None);

        Assert.Equal("abc", response.Echo);
    }
}
