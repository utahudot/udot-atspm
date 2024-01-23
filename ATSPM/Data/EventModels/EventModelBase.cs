using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace ATSPM.Data.EventModels
{
    public abstract class EventModelBase : ILocationLayer, ITimestamp
    {
        ///<inheritdoc/>
        [JsonIgnore]
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }


    public class IndiannaEvent : EventModelBase
    {
        public ushort EventCode { get; set; }
        public byte EventParam { get; set; }

        public override string ToString()
        {
            return $"{LocationIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }

    public class PedestrianCounter : EventModelBase
    {
        public ushort In { get; set; }
        public ushort Out { get; set; }
    }
}
