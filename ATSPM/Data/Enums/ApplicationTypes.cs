using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum ApplicationTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "ATSPM", Order = 1)]
        ATSPM = 1,
        [Display(Name = "SPMWatchDog", Order = 2)]
        SPMWatchDog = 2,
        [Display(Name = "Database Archive", Order = 3)]
        DatabaseArchive = 3,
        [Display(Name = "General Setting", Order = 4)]
        GeneralSetting = 4,
    }
}
