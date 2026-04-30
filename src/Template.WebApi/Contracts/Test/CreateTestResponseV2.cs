namespace Template.WebApi.Contracts.Test;

// V2 breaking change: ProcessedAt removed — consumers no longer receive processing timestamp.
public sealed record CreateTestResponseV2(string Message);
