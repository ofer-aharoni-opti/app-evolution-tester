using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Zibi;

public sealed class ZibiMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Domain → Application Responses. Property names match, so default mapping is enough.
        config.NewConfig<ZibiModel, CreateZibi.Response>();
        config.NewConfig<ZibiModel, GetZibi.Response>();
        config.NewConfig<ZibiModel, ListZibis.Item>();

        // UpdateZibi.Response.UpdatedAt is non-nullable: the model's UpdatedAt is always
        // populated by UpdateDetails before this mapping runs.
        config.NewConfig<ZibiModel, UpdateZibi.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
