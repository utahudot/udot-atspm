using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related signals
    /// </summary>
    public interface IRelatedSignals
    {
        /// <summary>
        /// Collection of signals
        /// </summary>
        ICollection<Signal> Signals { get; set; }
    }
}
