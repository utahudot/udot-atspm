using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum MovementTypes
    {
        [Display(Name = "Unknown", Order = 6)]
        NA = 0,
        [Display(Name = "Thru", Order = 3)]
        T = 1,
        [Display(Name = "Right", Order = 5)]
        R = 2,
        [Display(Name = "Left", Order = 1)]
        L = 3,
        [Display(Name = "Thru-Right", Order = 4)]
        TR = 4,
        [Display(Name = "Thru-Left", Order = 2)]
        TL = 5,
        [Display(Name = "Northwest", Order = 6)]
        NW = 6,
    }
}
