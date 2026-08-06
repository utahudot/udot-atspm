namespace Utah.Udot.Atspm.Services.ApiKeys
{
    /// <summary>
    /// Represents a lightweight summary of an API key, returned for user listings.
    /// </summary>
    public record ApiKeySummaryDto(int Id, string Name, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt);
}
