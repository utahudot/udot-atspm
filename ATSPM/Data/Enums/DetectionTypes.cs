using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum DetectionTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Basic", Order = 1)]
        B = 1,
        [Display(Name = "Advanced Count", Order = 2)]
        AC = 2,
        [Display(Name = "Advanced Speed", Order = 3)]
        AS = 3,
        [Display(Name = "Lane-by-lane Count", Order = 4)]
        LLC = 4,
        [Display(Name = "Lane-by-lane with Speed Restriction", Order = 5)]
        LLS = 5,
        [Display(Name = "Stop Bar Presence", Order = 6)]
        SBP = 6,
        [Display(Name = "Advanced Presence", Order = 7)]
        AP = 7,
        [Display(Name = "Passage", Order = 8)]
        P = 8,
        [Display(Name = "Demand", Order = 9)]
        D = 9,
        [Display(Name = "Intermediate Queue", Order = 10)]
        IQ = 10,
        [Display(Name = "Excessive Queue", Order = 11)]
        EQ = 11,
    }
}
