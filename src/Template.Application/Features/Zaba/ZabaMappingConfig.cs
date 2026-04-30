using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Zaba;

public sealed class ZabaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Domain → Application Responses. Property names match, so default mapping is enough.
        config.NewConfig<ZabaModel, CreateZaba.Response>();
        config.NewConfig<ZabaModel, GetZaba.Response>();
        config.NewConfig<ZabaModel, ListZabas.Item>();

        // UpdateZaba.Response.UpdatedAt is non-nullable: the model's UpdatedAt is always
        // populated by UpdateDetails before this mapping runs.
        config.NewConfig<ZabaModel, UpdateZaba.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
