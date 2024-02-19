using ATSPM.Data.Enums;

#nullable disable

namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Indiana Traffic Signal Hi Resolution Data Logger Enumerations
    /// <seealso cref="https://docs.lib.purdue.edu/jtrpdata/4/"/>
    /// </summary>
    public class IndianaEvent : EventLogModelBase
    {
        /// <summary>
        /// Event code from <see cref="DataLoggerEnum"/>
        /// </summary>
        public DataLoggerEnum EventCode { get; set; }

        /// <summary>
        /// Event parameter that is specific to <see cref="EventCode"/>
        /// </summary>
        public short EventParam { get; set; }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is IndianaEvent @event &&
                   LocationIdentifier == @event.LocationIdentifier &&
                   Timestamp == @event.Timestamp &&
                   EventCode == @event.EventCode &&
                   EventParam == @event.EventParam;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(LocationIdentifier, Timestamp, EventCode, EventParam);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{EventCode}-{EventParam}";
        }
    }
}
