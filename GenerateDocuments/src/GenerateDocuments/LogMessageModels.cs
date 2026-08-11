namespace AtspmDocsGenerator;

public sealed record LogMessageDefinition(
    int EventId,
    string EventName,
    string Level,
    string? Summary,
    string SourcePath,
    int SourceLine);

public sealed record LogMessageGenerationResult(int MessageCount, int DuplicateEventIdCount);
