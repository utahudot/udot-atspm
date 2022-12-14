using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ATSPM.Data.Enums
{
    public enum MetricFilterTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        Unknown,
        [Display(Name = "Signal Id", Order = 1)]
        SignalId,
        [Display(Name = "Primary Name", Order = 2)]
        PrimaryName,
        [Display(Name = "Secondary Name", Order = 3)]
        SecondaryName,
        [Display(Name = "Agency", Order = 4)]
        Agency
    }
}
