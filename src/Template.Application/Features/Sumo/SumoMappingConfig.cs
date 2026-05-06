using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Sumo;

public sealed class SumoMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SumoModel, CreateSumo.Response>();
        config.NewConfig<SumoModel, GetSumo.Response>();
        config.NewConfig<SumoModel, ListSumos.Item>();

        config.NewConfig<SumoModel, UpdateSumo.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
