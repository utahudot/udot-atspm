using System.ComponentModel.DataAnnotations;

namespace Utah.Udot.Atspm.Enums.SpeedManagement
{
    public enum SourceEnum
    {
        [Display(Name = "ATSPM")]
        ATSPM = 1,
        [Display(Name = "PEMS")]
        PEMS = 2,
        [Display(Name = "ClearGuide")]
        ClearGuide = 3
    }
}
