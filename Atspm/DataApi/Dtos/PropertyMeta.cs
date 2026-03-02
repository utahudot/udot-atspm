namespace Utah.Udot.ATSPM.DataApi.Dtos
{
    /// <summary>
    /// Represents metadata describing a single property within a data type.
    /// Used to expose property names and their associated XML documentation
    /// when generating API metadata.
    /// </summary>
    public record PropertyMeta
    {
        /// <summary>
        /// The name of the property being described.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The XML documentation summary associated with the property, if available.
        /// </summary>
        public string? Description { get; init; }
    }

    /// <summary>
    /// Represents metadata describing a data type, including its own documentation
    /// and the metadata for each of its public properties.
    /// </summary>
    public record DataTypeMeta : PropertyMeta
    {
        /// <summary>
        /// The collection of properties defined on the data type, each including
        /// its name and associated XML documentation summary.
        /// </summary>
        public IReadOnlyList<PropertyMeta> Properties { get; init; } = [];
    }

}
