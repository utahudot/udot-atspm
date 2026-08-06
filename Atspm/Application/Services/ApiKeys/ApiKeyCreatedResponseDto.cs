namespace Utah.Udot.Atspm.Services.ApiKeys
{
    /// <summary>
    /// Represents the response containing the generated raw API key and a helper instruction message.
    /// </summary>
    public record ApiKeyCreatedResponseDto(string Key, string Message);
}
