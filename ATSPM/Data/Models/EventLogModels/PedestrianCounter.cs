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
        public override bool Equals(object obj)
        {
            return obj is PedestrianCounter counter &&
                   LocationIdentifier == counter.LocationIdentifier &&
                   Timestamp == counter.Timestamp &&
                   In == counter.In &&
                   Out == counter.Out;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, In, Out);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{In}-{Out}";
        }
    }
}
