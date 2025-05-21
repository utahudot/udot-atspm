using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Messaging
{
    public class EventBatchEnvelope
    {
        [Required]
        public string LocationIdentifier { get; set; }

        [Required]
        public int DeviceId { get; internal set; }

        [Required]
        public string DataType { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Required]
        public JToken Items { get; set; }
    }

}
