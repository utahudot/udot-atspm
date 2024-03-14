using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Models.EventLogModels
{
    /// <summary>
    /// Event log model base for models used in logging Atspm device data
    /// </summary>
    public abstract class EventLogModelBase : ILocationLayer, ITimestamp
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }
}
