namespace Template.Infrastructure.Dto;

public sealed record SumoDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
