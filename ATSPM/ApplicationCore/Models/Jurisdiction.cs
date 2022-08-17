using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public partial class Jurisdiction : ATSPMModelBase
    {
        public int Id { get; set; }
        public string JurisdictionName { get; set; }
        public string MPO { get; set; }
        public string CountyParish { get; set; }
        public string OtherPartners { get; set; }
        public virtual ICollection<Signal> Signals { get; set; }
    }
}
