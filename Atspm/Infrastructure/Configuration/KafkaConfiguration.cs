using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; } = default!;
        public string Topic { get; set; } = default!;
    }
}
