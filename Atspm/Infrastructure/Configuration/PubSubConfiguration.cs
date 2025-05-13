using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class PubSubConfiguration
    {
        public string ProjectId { get; set; } = default!;
        public string TopicId { get; set; } = default!;
    }
}
