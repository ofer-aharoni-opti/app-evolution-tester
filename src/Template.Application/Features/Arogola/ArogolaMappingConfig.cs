using Mapster;
using Template.Domain.Models;

namespace Template.Application.Features.Arogola;

public sealed class ArogolaMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ArogolaModel, CreateArogola.Response>();
        config.NewConfig<ArogolaModel, GetArogola.Response>();
        config.NewConfig<ArogolaModel, ListArogolas.Item>();

        config.NewConfig<ArogolaModel, UpdateArogola.Response>()
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt!.Value);
    }
}
