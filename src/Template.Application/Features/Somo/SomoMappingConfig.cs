using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Somo;

public sealed class SomoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Domain → Application Responses. Property names match, so default mapping is enough.
        config.NewConfig<SomoModel, CreateSomo.Response>();
        config.NewConfig<SomoModel, GetSomo.Response>();
        config.NewConfig<SomoModel, ListSomos.Item>();

        // UpdateSomo.Response.UpdatedAt is non-nullable: the model's UpdatedAt is always
        // populated by UpdateDetails before this mapping runs.
        config.NewConfig<SomoModel, UpdateSomo.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
