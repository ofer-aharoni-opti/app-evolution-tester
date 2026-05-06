using Mapster;
using Template.Application.Features.Sumo;

namespace Template.WebApi.Contracts.Sumo;

public sealed class SumoContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSumoRequest, CreateSumo.Command>();
    }
}
