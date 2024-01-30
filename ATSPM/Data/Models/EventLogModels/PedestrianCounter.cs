#nullable disable


namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Generic pedestrian counter events
    /// </summary>
    public class PedestrianCounter : EventLogModelBase
    {
        /// <summary>
        /// Input count
        /// </summary>
        public ushort In { get; set; }
        
        /// <summary>
        /// Output count
        /// </summary>
        public ushort Out { get; set; }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{In}-{Out}";
        }
    }
}
