using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Enums
{
    public enum ActionTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Actuated Coord.")]
        ActuatedCoord = 1,
        [Display(Name = "Coord On/Off")]
        CoordOnOff = 2,
        [Display(Name = "Cycle Length")]
        CycleLength = 3,
        [Display(Name = "Detector Issue")]
        DetectorIssue = 4,
        [Display(Name = "Offset")]
        Offset = 5,
        [Display(Name = "Sequence")]
        Sequence = 6,
        [Display(Name = "Time Of Day")]
        TimeOfDay = 7,
        [Display(Name = "Other")]
        Other = 8,
        [Display(Name = "All-Red Interval")]
        AllRedInterval = 9,
        [Display(Name = "Modeling")]
        Modeling = 10,
        [Display(Name = "Traffic Study")]
        TrafficStudy = 11,
        [Display(Name = "Yellow Interval")]
        YellowInterval = 12,
        [Display(Name = "Force Off Type")]
        ForceOffType = 13,
        [Display(Name = "Split Adjustment")]
        SplitAdjustment = 14,
        [Display(Name = "Manual Command")]
        ManualCommand = 15
    }
}
