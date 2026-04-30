using Template.Infrastructure.MultiTenancy;

namespace Template.Infrastructure.UnitTests.MultiTenancy;

public sealed class TenantApplicationContextTests
{
    [Fact]
    public void Context_is_null_before_initialization()
    {
        var sut = new TenantApplicationContext();
        Assert.Null(sut.Context);
    }

    [Fact]
    public void Initialize_sets_context_with_tenant_id_and_username()
    {
        var sut = new TenantApplicationContext();

        sut.Initialize(42, "test-user");

        Assert.NotNull(sut.Context);
        Assert.Equal(42, sut.Context.TenantId);
        Assert.Equal("test-user", sut.Context.UserName);
    }

    [Fact]
    public void Initialize_sets_context_with_null_username()
    {
        var sut = new TenantApplicationContext();

        sut.Initialize(1, null);

        Assert.NotNull(sut.Context);
        Assert.Equal(1, sut.Context.TenantId);
        Assert.Null(sut.Context.UserName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Initialize_throws_for_invalid_tenant_id(int invalidId)
    {
        var sut = new TenantApplicationContext();

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Initialize(invalidId, null));
    }
}
