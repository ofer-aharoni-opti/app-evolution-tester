using Mapster;
using Template.Application.Features.Somo;

namespace Template.WebApi.Contracts.Somo;

public sealed class SomoContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // WebApi request → Application command. Property names match, so default mapping is enough.
        config.NewConfig<CreateSomoRequest, CreateSomo.Command>();
    }
}
