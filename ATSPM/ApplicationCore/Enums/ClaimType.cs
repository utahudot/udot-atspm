using System.ComponentModel;

namespace ATSPM.Application.Enums
{
    public enum ClaimTypes
    {
        [Description("Admin")]
        Admin,

        [Description("User:View")]
        UserView,

        [Description("User:Edit")]
        UserEdit,

        [Description("User:Delete")]
        UserDelete,

        [Description("Role:View")]
        RoleView,

        [Description("Role:Edit")]
        RoleEdit,

        [Description("Role:Delete")]
        RoleDelete,

        [Description("LocationConfiguration:View")]
        LocationConfigurationView,

        [Description("LocationConfiguration:Edit")]
        LocationConfigurationEdit,

        [Description("LocationConfiguration:Delete")]
        LocationConfigurationDelete,

        [Description("GeneralConfiguration:View")]
        GeneralConfigurationView,

        [Description("GeneralConfiguration:Edit")]
        GeneralConfigurationEdit,

        [Description("GeneralConfiguration:Delete")]
        GeneralConfigurationDelete,

        [Description("Data:View")]
        DataView,

        [Description("Data:Edit")]
        DataEdit,

        [Description("Watchdog:View")]
        WatchdogView,

        [Description("Report:View")]
        ReportView
    }
}

