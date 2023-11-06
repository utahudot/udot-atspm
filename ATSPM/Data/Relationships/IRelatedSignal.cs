using ATSPM.Data.Models;

namespace ATSPM.Data.Relationships
{
    /// <summary>
    /// Related signal
    /// </summary>
    public interface IRelatedSignal
    {
        /// <summary>
        /// Related signal
        /// </summary>
        int SignalId { get; set; }
        
        /// <summary>
        /// Signal
        /// </summary>
        Signal Signal { get; set; }
    }
}
