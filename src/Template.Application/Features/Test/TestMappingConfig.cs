using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Test;

public sealed class TestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ProcessedMessage → Message requires explicit mapping (names differ).
        // IsProcessed and ProcessedAt auto-map by matching property names.
        config.NewConfig<TestModel, CreateTest.Response>()
            .Map(dest => dest.Message, src => src.ProcessedMessage);

        config.NewConfig<TestModel, GetTest.Response>()
            .Map(dest => dest.Echo, src => src.Value);
    }
}
