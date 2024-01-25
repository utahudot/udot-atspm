using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;

#nullable disable

namespace ATSPM.Data.Models.EventModels
{
    /// <summary>
    /// Event model base for models used in logging Atspm device logging data
    /// </summary>
    public abstract class AtspmEventModelBase : ILocationLayer, ITimestamp
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }
}
