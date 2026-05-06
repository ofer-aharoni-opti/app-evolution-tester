namespace Template.Domain.Models;

public sealed class BananaModel
{
    public required Guid Id { get; init; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }

    public static BananaModel Create(string name, string description)
    {
        var model = new BananaModel
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
        };
        model.SetDetails(name, description);
        return model;
    }

    public static BananaModel Hydrate(
        Guid id,
        string name,
        string description,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        var model = new BananaModel
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
