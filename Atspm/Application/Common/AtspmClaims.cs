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
            public const string ApiKeysCreate = "ApiKeys:Create";
            public const string ApiKeysView = "ApiKeys:View";
            public const string ApiKeysRevoke = "ApiKeys:Revoke";

            // Data
            public const string DataEdit = "Data:Edit";
            public const string DataView = "Data:View";

            // General Configuration
            public const string GeneralConfigurationsDelete = "GeneralConfigurations:Delete";
            public const string GeneralConfigurationsEdit = "GeneralConfigurations:Edit";
            public const string GeneralConfigurationsView = "GeneralConfigurations:View";

            // Location Configuration
            public const string LocationConfigurationsDelete = "LocationConfigurations:Delete";
            public const string LocationConfigurationsEdit = "LocationConfigurations:Edit";
            public const string LocationConfigurationsView = "LocationConfigurations:View";

            // Reports
            public const string ReportView = "Report:View";

            // Roles
            public const string RolesDelete = "Roles:Delete";
            public const string RolesEdit = "Roles:Edit";
            public const string RolesView = "Roles:View";

            // Users
            public const string UsersDelete = "Users:Delete";
            public const string UsersEdit = "Users:Edit";
            public const string UsersView = "Users:View";

            // Watchdog
            public const string WatchdogView = "Watchdog:View";
        }
    }
}
