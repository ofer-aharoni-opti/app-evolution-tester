namespace Template.Domain.Models;

/// <summary>
/// Domain model encapsulates business rules and state transitions.
/// All business logic belongs here — not in handlers or controllers.
/// </summary>
public sealed class ZabaModel
{
    public required Guid Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Factory keeps construction invariants (Id, CreatedAt) in one place.
    /// </summary>
    public static ZabaModel Create(string name, string description)
    {
        var model = new ZabaModel
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        model.SetDetails(name, description);
        return model;
    }

    /// <summary>
    /// Reconstructs an existing model from persisted state — preserves original timestamps.
    /// Used only by repositories; never call from handlers or controllers.
    /// </summary>
    public static ZabaModel Hydrate(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        var model = new ZabaModel
        {
            Id = id,
            CreatedAt = createdAt,
        };
        model.SetDetails(name, description);
        model.UpdatedAt = updatedAt;
        return model;
    }

    /// <summary>
    /// State-changing behavior is encapsulated in methods, not exposed via public setters.
    /// UpdatedAt is touched only on real updates so reads can distinguish created vs. modified.
    /// </summary>
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
