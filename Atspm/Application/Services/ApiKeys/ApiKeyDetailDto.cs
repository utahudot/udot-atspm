namespace Utah.Udot.Atspm.Services.ApiKeys
{
    /// <summary>
    /// Represents a detailed description of an API key, including its owner's identifier.
    /// </summary>
    public record ApiKeyDetailDto(int Id, string Name, string OwnerId, DateTimeOffset CreatedAt, DateTimeOffset? ExpiresAt);
}
