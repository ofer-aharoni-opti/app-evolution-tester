namespace Template.Domain.Models;

/// <summary>
/// Domain model encapsulates business rules and state transitions.
/// All business logic belongs here — not in handlers or controllers.
/// </summary>
public sealed class ArogolaModel
{
    public required Guid Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }

    public static ArogolaModel Create(string name, string description)
    {
        var model = new ArogolaModel
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        model.SetDetails(name, description);
        return model;
    }

    public static ArogolaModel Hydrate(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        var model = new ArogolaModel
        {
            Id = id,
            CreatedAt = createdAt,
        };
        model.SetDetails(name, description);
        model.UpdatedAt = updatedAt;
        return model;
    }

    public void UpdateDetails(string name, string description)
    {
        SetDetails(name, description);
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name.Trim();
        Description = description ?? string.Empty;
    }
}
