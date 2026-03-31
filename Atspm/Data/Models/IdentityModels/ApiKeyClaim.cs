namespace Utah.Udot.Atspm.Data.Models.IdentityModels
{
    /// <summary>
    /// Represents an individual claim or permission assigned to an <see cref="ApiKey"/>.
    /// </summary>
    public class ApiKeyClaim
    {
        /// <summary>
        /// Gets or sets the unique identifier for the API key claim.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the foreign key identifier for the associated <see cref="ApiKey"/>.
        /// </summary>
        public int ApiKeyId { get; set; }

        /// <summary>
        /// Gets or sets the type of the claim (e.g., "role", "permission", or "scope").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value associated with the claim type (e.g., "Admin", "Read-Only").
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
