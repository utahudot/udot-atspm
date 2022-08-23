using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum LaneTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Vehicle")]
        V = 1,
        [Display(Name = "Bike")]
        Bike = 2,
        [Display(Name = "Pedestrian")]
        Ped = 3,
        [Display(Name = "Exit")]
        E = 4,
        [Display(Name = "Light Rail Transit")]
        LRT = 5,
        [Display(Name = "Bus")]
        Bus = 6,
        [Display(Name = "High Occupancy Vehicle")]
        HDV = 7,
    }
}
