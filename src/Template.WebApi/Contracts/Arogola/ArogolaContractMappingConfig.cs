using Mapster;
using Template.Application.Features.Arogola;

namespace Template.WebApi.Contracts.Arogola;

public sealed class ArogolaContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateArogolaRequest, CreateArogola.Command>();
    }
}
