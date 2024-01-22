using ATSPM.Data.Interfaces;
using ATSPM.Domain.Common;
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
        public string LocationIdentifier { get; set; }

        ///<inheritdoc/>
        public DateTime Timestamp { get; set; }
    }


    public class IndiannaEvent : EventModelBase
    {
        public int EventCode { get; set; }
        public int EventParam { get; set; }

        public override string ToString()
        {
            return $"{LocationIdentifier}-{EventCode}-{EventParam}-{Timestamp}";
        }
    }

    public class PedstrianCounter : EventModelBase
    {
        public int In { get; set; }
        public int Out { get; set; }
    }
}
