using Mapster;
using Template.Application.Features.Banana;

namespace Template.WebApi.Contracts.Banana;

public sealed class BananaContractMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBananaRequest, CreateBanana.Command>();
    }
}
