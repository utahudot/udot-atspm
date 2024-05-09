#nullable disable


namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Generic speed events
    /// </summary>
    public class SpeedEvent : EventLogModelBase
    {
        //TODO: is this the database id or the detector channel?
        /// <summary>
        /// Detector id
        /// </summary>
        public string DetectorId { get; set; }

        /// <summary>
        /// Miles per hour
        /// </summary>
        public int Mph { get; set; }
        
        /// <summary>
        /// Kilometers per hour
        /// </summary>
        public int Kph { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is SpeedEvent @event &&
                   LocationIdentifier == @event.LocationIdentifier &&
                   Timestamp == @event.Timestamp &&
                   DetectorId == @event.DetectorId &&
                   Mph == @event.Mph &&
                   Kph == @event.Kph;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, DetectorId, Mph, Kph);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{DetectorId}-{Mph}-{Kph}";
        }
    }
}
