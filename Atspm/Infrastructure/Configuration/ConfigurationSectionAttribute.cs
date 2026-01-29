namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Identifies a class as representing a configuration section and provides
    /// metadata used for documentation and configuration binding.
    /// </summary>
    /// <remarks>
    /// Apply this attribute to a configuration options class to associate it with
    /// a named configuration section. The optional description can be used to
    /// provide human‑readable documentation for generated configuration reference
    /// materials.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ConfigurationSectionAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the configuration section associated with the target class.
        /// </summary>
        public string SectionName { get; }

        /// <summary>
        /// Gets an optional human‑readable description of the configuration section.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationSectionAttribute"/> class.
        /// </summary>
        /// <param name="sectionName">
        /// The name of the configuration section. This value is typically used when binding
        /// configuration from <c>appsettings.json</c>, environment variables, or other providers.
        /// </param>
        /// <param name="description">
        /// An optional description of the configuration section, used primarily for documentation
        /// generation or tooling support.
        /// </param>
        public ConfigurationSectionAttribute(string sectionName, string? description = null)
        {
            SectionName = sectionName;
            Description = description;
        }
    }
}
