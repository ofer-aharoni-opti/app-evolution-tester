using System.Runtime.CompilerServices;
using Mapster;

namespace Template.Application.UnitTests;

internal static class TestInitializer
{
    internal static MapsterMapper.Mapper Mapper { get; private set; } = null!;

    [ModuleInitializer]
    internal static void Initialize()
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(AssemblyReference).Assembly);
        Mapper = new MapsterMapper.Mapper(config);
    }
}
