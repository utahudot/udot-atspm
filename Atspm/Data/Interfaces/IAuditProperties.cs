namespace Utah.Udot.Atspm.Data.Interfaces
{
    /// <summary>
    /// Interface for audit properties to track entity changes.
    /// </summary>
    public interface IAuditProperties
    {
        /// <summary>
        /// Gets or sets the date and time when the entity was created.
        /// </summary>
        DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the entity was last modified.
        /// </summary>
        DateTime? Modified { get; set; }

        /// <summary>
        /// Gets or sets the user who created the entity.
        /// </summary>
        string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the user who last modified the entity.
        /// </summary>
        string? ModifiedBy { get; set; }
    }
}
