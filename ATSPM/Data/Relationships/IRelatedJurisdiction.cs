using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related jurisdiction
    /// </summary>
    public interface IRelatedJurisdiction
    {
        /// <summary>
        /// Related jurisdiction
        /// </summary>
        int? JurisdictionId { get; set; }
        
        /// <summary>
        /// Jurisdiction
        /// </summary>
        Jurisdiction? Jurisdiction { get; set; }
    }
}
