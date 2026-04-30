namespace Template.Domain.Models;

/// <summary>
/// Domain model encapsulates business rules and state transitions.
/// All business logic belongs here — not in handlers or controllers.
/// </summary>
public sealed class TestModel
{
    public required string Value { get; init; }
    public bool IsProcessed { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// Derived property: business rules like formatting and transformation belong in the domain model.
    /// </summary>
    public string ProcessedMessage => $"handled:{Value}";

    /// <summary>
    /// State-changing behavior is encapsulated in methods, not exposed via public setters.
    /// This ensures invariants are maintained by the domain model itself.
    /// </summary>
    public void MarkAsProcessed()
    {
        IsProcessed = true;
        ProcessedAt = DateTime.UtcNow;
    }
}
