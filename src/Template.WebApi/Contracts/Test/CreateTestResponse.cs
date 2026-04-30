namespace Template.WebApi.Contracts.Test;

// WebApi response contract — the shape exposed to API consumers.
// This differs from Application's CreateTest.Response:
//   - ProcessedAt is formatted as an ISO 8601 string (API-friendly)
//   - IsProcessed is omitted (internal detail not relevant to consumers)
// Introduce a WebApi response contract only when the API shape diverges from the Application Response.
public sealed record CreateTestResponse(string Message, string ProcessedAt);
