using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Banana;

public sealed class BananaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BananaModel, CreateBanana.Response>();
        config.NewConfig<BananaModel, GetBanana.Response>();
        config.NewConfig<BananaModel, ListBananas.Item>();

        config.NewConfig<BananaModel, UpdateBanana.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
