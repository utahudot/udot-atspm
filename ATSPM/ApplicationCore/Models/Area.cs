using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class Area : ATSPMModelBase
    {
        public int Id { get; set; }
        public string AreaName { get; set; }
        public virtual ICollection<Signal> Signals { get; set; }
        public List<int> SignalIds { get; set; }
    }
}
