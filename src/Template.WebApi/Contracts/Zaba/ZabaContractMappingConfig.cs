using Mapster;
using Template.Application.Features.Zaba;

namespace Template.WebApi.Contracts.Zaba;

public sealed class ZabaContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // WebApi request → Application command. Property names match, so default mapping is enough.
        config.NewConfig<CreateZabaRequest, CreateZaba.Command>();
    }
}
