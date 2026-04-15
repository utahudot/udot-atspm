namespace Utah.Udot.ATSPM.IdentityApi.Dto
{
    /// <summary>
    /// Data transfer object used for providing the necessary information to create a new API key.
    /// </summary>
    public class CreateApiKeyDto
    {
        /// <summary>
        /// Gets or sets a descriptive name for the API key to help identify its purpose.
        /// </summary>
        /// <example>Internal Integration Service</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the optional expiration date and time for the API key. 
        /// If null, the key may be treated as having no expiration, depending on system policy.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the list of roles or permissions assigned to this API key.
        /// </summary>
        /// <value>A list of strings representing role names.</value>
        public List<string> Roles { get; set; }
    }
}
