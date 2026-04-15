namespace Utah.Udot.Atspm.Common
{
    public static class AtspmClaims
    {
        public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string ApiKeyAdmin = "ApiKeyAdmin";
            public const string DataAdmin = "DataAdmin";
            public const string GeneralConfigurationAdmin = "GeneralConfigurationAdmin";
            public const string LocationConfigurationAdmin = "LocationConfigurationAdmin";
            public const string ReportAdmin = "ReportAdmin";
            public const string RoleAdmin = "RoleAdmin";
            public const string UserAdmin = "UserAdmin";
            public const string WatchdogSubscriber = "WatchdogSubscriber";
        }

        public static class Permissions
        {
            // admin
            public const string Admin = "Admin";

            // ApiKey
            public const string ApiKeyCreate = "ApiKey:Create";
            public const string ApiKeyView = "ApiKey:View";
            public const string ApiKeyRevoke = "ApiKey:Revoke";

            // Data
            public const string DataEdit = "Data:Edit";
            public const string DataView = "Data:View";

            // General Configuration
            public const string GeneralConfigurationDelete = "GeneralConfiguration:Delete";
            public const string GeneralConfigurationEdit = "GeneralConfiguration:Edit";
            public const string GeneralConfigurationView = "GeneralConfiguration:View";

            // Location Configuration
            public const string LocationConfigurationDelete = "LocationConfiguration:Delete";
            public const string LocationConfigurationEdit = "LocationConfiguration:Edit";
            public const string LocationConfigurationView = "LocationConfiguration:View";

            // Reports
            public const string ReportView = "Report:View";

            // Roles
            public const string RoleDelete = "Role:Delete";
            public const string RoleEdit = "Role:Edit";
            public const string RoleView = "Role:View";

            // Users
            public const string UserDelete = "User:Delete";
            public const string UserEdit = "User:Edit";
            public const string UserView = "User:View";

            // Watchdog
            public const string WatchdogView = "Watchdog:View";
        }
    }
}
