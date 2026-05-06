using Mapster;
using Template.Application.Features.Zibi;

namespace Template.WebApi.Contracts.Zibi;

public sealed class ZibiContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // WebApi request → Application command. Property names match, so default mapping is enough.
        config.NewConfig<CreateZibiRequest, CreateZibi.Command>();
    }
}
