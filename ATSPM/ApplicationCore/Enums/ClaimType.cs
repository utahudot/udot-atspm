using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Application.Enums
{
    public enum ClaimTypes
    {
        [Display(Name = "Admin")]
        Admin,

        [Display(Name = "User:View")]
        UserView,

        [Display(Name = "User:Edit")]
        UserEdit,

        [Display(Name = "User:Delete")]
        UserDelete,

        [Display(Name = "Role:View")]
        RoleView,

        [Display(Name = "Role:Edit")]
        RoleEdit,

        [Display(Name = "Role:Delete")]
        RoleDelete,

        [Display(Name = "LocationConfiguration:View")]
        LocationConfigurationView,

        [Display(Name = "LocationConfiguration:Edit")]
        LocationConfigurationEdit,

        [Display(Name = "LocationConfiguration:Delete")]
        LocationConfigurationDelete,

        [Display(Name = "GeneralConfiguration:View")]
        GeneralConfigurationView,

        [Display(Name = "GeneralConfiguration:Edit")]
        GeneralConfigurationEdit,

        [Display(Name = "GeneralConfiguration:Delete")]
        GeneralConfigurationDelete,

        [Display(Name = "Data:View")]
        DataView,

        [Display(Name = "Data:Edit")]
        DataEdit,

        [Display(Name = "Watchdog:View")]
        WatchdogView,

        [Display(Name = "Report:View")]
        ReportView
    }
}

