namespace Template.Infrastructure.Dto;

// Persistence DTO — flat representation of what gets stored.
// Maps to/from the domain model inside the repository.
public sealed record ZabaDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
