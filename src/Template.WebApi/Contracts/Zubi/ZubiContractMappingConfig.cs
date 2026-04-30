using Mapster;
using Template.Application.Features.Zubi;

namespace Template.WebApi.Contracts.Zubi;

public sealed class ZubiContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // WebApi request → Application command. Property names match, so default mapping is enough.
        config.NewConfig<CreateZubiRequest, CreateZubi.Command>();
    }
}
