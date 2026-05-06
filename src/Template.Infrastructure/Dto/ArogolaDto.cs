namespace Template.Infrastructure.Dto;

public sealed record ArogolaDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
