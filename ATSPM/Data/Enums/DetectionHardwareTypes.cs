using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum DetectionHardwareTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Wavetronix Matrix", Order = 1)]
        WavetronixMatrix = 1,
        [Display(Name = "Wavetronix Advance", Order = 2)]
        WavetronixAdvance = 2,
        [Display(Name = "Inductive Loops", Order = 3)]
        InductiveLoops = 3,
        [Display(Name = "Sensys", Order = 4)]
        Sensys = 4,
        [Display(Name = "Video", Order = 5)]
        Video = 5,
        [Display(Name = "FLIR: Thermal Camera", Order = 6)]
        FLIRThermalCamera = 6,
    }
}
