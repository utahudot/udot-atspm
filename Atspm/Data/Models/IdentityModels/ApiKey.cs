namespace Utah.Udot.Atspm.Data.Models.IdentityModels
{
    /// <summary>
    /// Represents an API key used for authentication and authorization within the system.
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// Gets or sets the unique identifier for the API key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a descriptive name for the API key to help users identify it.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed version of the API key. 
        /// The raw key should never be stored in plain text.
        /// </summary>
        public string KeyHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier of the user or service that owns this API key.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the API key was created. 
        /// Defaults to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the expiration date and time for the API key. 
        /// If null, the key does not expire.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the API key has been manually revoked.
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Gets or sets the list of specific permissions or claims associated with this API key.
        /// </summary>
        public List<ApiKeyClaim> Claims { get; set; } = new List<ApiKeyClaim>();
    }
}
