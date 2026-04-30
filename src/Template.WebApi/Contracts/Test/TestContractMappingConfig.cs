using Mapster;
using Template.Application.Features.Test;

namespace Template.WebApi.Contracts.Test;

public sealed class TestContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Application Response → WebApi contract:
        // formats DateTime? as ISO 8601 string for API consumers.
        config.NewConfig<CreateTest.Response, CreateTestResponse>()
            .Map(dest => dest.ProcessedAt,
                 src => src.ProcessedAt != null
                     ? src.ProcessedAt.Value.ToString("O")
                     : string.Empty);
    }
}
