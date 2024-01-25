using ATSPM.Data.Enums;
using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Models.EventModels
{
    /// <summary>
    /// Event model base for models used in logging Atspm device logging data
    /// </summary>
    public abstract class EventModelBase : ILocationLayer, ITimestamp
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Indiana Traffic Signal Hi Resolution Data Logger Enumerations
    /// <seealso cref="https://docs.lib.purdue.edu/jtrpdata/4/"/>
    /// </summary>
    public class IndiannaEvent : EventModelBase
    {
        /// <summary>
        /// Event code from <see cref="DataLoggerEnum"/>
        /// </summary>
        public DataLoggerEnum EventCode { get; set; }
        
        /// <summary>
        /// Event parameter that is specific to <see cref="EventCode"/>
        /// </summary>
        public byte EventParam { get; set; }

        ///<inheritdoc/>
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{EventCode}-{EventParam}";
        }
    }

    /// <summary>
    /// Generic speed events
    /// </summary>
    public class SpeedEvent : EventModelBase
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
        public override string ToString()
        {
            return $"{LocationIdentifier}-{Timestamp}-{DetectorId}-{Mph}-{Kph}";
        }
    }

    /// <summary>
    /// Generic pedestrian counter events
    /// </summary>
    public class PedestrianCounter : EventModelBase
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
