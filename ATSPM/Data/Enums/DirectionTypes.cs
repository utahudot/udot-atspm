using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum DirectionTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Northbound", Order = 3)]
        NB = 1,
        [Display(Name = "Southbound", Order = 4)]
        SB = 2,
        [Display(Name = "Eastbound", Order = 1)]
        EB = 3,
        [Display(Name = "Westbound", Order = 2)]
        WB = 4,
        [Display(Name = "Northeast", Order = 5)]
        NE = 5,
        [Display(Name = "Northwest", Order = 6)]
        NW = 6,
        [Display(Name = "Southeast", Order = 7)]
        SE = 7,
        [Display(Name = "Southwest", Order = 8)]
        SW = 8
    }
}
