using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Zubi;

public sealed class ZubiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Domain → Application Responses. Property names match, so default mapping is enough.
        config.NewConfig<ZubiModel, CreateZubi.Response>();
        config.NewConfig<ZubiModel, GetZubi.Response>();
        config.NewConfig<ZubiModel, ListZubis.Item>();

        // UpdateZubi.Response.UpdatedAt is non-nullable: the model's UpdatedAt is always
        // populated by UpdateDetails before this mapping runs.
        config.NewConfig<ZubiModel, UpdateZubi.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
