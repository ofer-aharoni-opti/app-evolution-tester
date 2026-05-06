namespace Template.Infrastructure.Dto;

public sealed record BananaDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
