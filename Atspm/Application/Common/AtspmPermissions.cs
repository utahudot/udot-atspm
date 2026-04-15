namespace Utah.Udot.Atspm.Common
{
    /// <summary>
    /// Provides a centralized source of truth for ATSPM identity constants, roles, and functional permissions.
    /// This class handles the mapping between raw permission strings and authorization policy names.
    /// </summary>
    public static class AtspmPermissions
    {
        /// <summary>
        /// The standard claim type URI used for role-based authorization.
        /// </summary>
        public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        /// <summary>
        /// Contains the static string identifiers for application-level roles.
        /// These are typically assigned to users to group multiple permissions.
        /// </summary>
        public static class Roles
        {
            /// <summary>System-wide administrator with access to all features.</summary>
            public const string Admin = "Admin";
            /// <summary>Administrator for managing API keys.</summary>
            public const string ApiKeyAdmin = "ApiKeyAdmin";
            /// <summary>Administrator for high-level data management.</summary>
            public const string DataAdmin = "DataAdmin";
            /// <summary>Administrator for system-wide general configurations.</summary>
            public const string GeneralConfigurationAdmin = "GeneralConfigurationAdmin";
            /// <summary>Administrator for location-specific configurations.</summary>
            public const string LocationConfigurationAdmin = "LocationConfigurationAdmin";
            /// <summary>Administrator for report generation and management.</summary>
            public const string ReportAdmin = "ReportAdmin";
            /// <summary>Administrator for managing security roles.</summary>
            public const string RoleAdmin = "RoleAdmin";
            /// <summary>Administrator for user account management.</summary>
            public const string UserAdmin = "UserAdmin";
            /// <summary>Subscriber role for watchdog monitoring alerts.</summary>
            public const string WatchdogSubscriber = "WatchdogSubscriber";
        }

        /// <summary>
        /// Contains the granular functional permission constants. 
        /// Format: "Category:Action" (e.g., "Users:View").
        /// </summary>
        public static class Permissions
        {
            // admin
            /// <summary>Master permission for all administrative tasks.</summary>
            public const string Admin = "Admin";

            // ApiKey
            /// <summary>Permission to generate new API keys.</summary>
            public const string ApiKeysCreate = "ApiKeys:Create";
            /// <summary>Permission to view existing API keys.</summary>
            public const string ApiKeysView = "ApiKeys:View";
            /// <summary>Permission to revoke active API keys.</summary>
            public const string ApiKeysRevoke = "ApiKeys:Revoke";

            // Data
            /// <summary>Permission to modify ATSPM data records.</summary>
            public const string DataEdit = "Data:Edit";
            /// <summary>Permission to view ATSPM data records.</summary>
            public const string DataView = "Data:View";

            // General Configuration
            /// <summary>Permission to delete general system configurations.</summary>
            public const string GeneralConfigurationsDelete = "GeneralConfigurations:Delete";
            /// <summary>Permission to modify general system configurations.</summary>
            public const string GeneralConfigurationsEdit = "GeneralConfigurations:Edit";
            /// <summary>Permission to view general system configurations.</summary>
            public const string GeneralConfigurationsView = "GeneralConfigurations:View";

            // Location Configuration
            /// <summary>Permission to delete specific location configurations.</summary>
            public const string LocationConfigurationsDelete = "LocationConfigurations:Delete";
            /// <summary>Permission to modify specific location configurations.</summary>
            public const string LocationConfigurationsEdit = "LocationConfigurations:Edit";
            /// <summary>Permission to view specific location configurations.</summary>
            public const string LocationConfigurationsView = "LocationConfigurations:View";

            // Reports
            /// <summary>Permission to access and view system reports.</summary>
            public const string ReportView = "Report:View";

            // Roles
            /// <summary>Permission to delete security roles.</summary>
            public const string RolesDelete = "Roles:Delete";
            /// <summary>Permission to edit security role definitions.</summary>
            public const string RolesEdit = "Roles:Edit";
            /// <summary>Permission to view security role definitions.</summary>
            public const string RolesView = "Roles:View";

            // Users
            /// <summary>Permission to delete user accounts.</summary>
            public const string UsersDelete = "Users:Delete";
            /// <summary>Permission to edit user account details.</summary>
            public const string UsersEdit = "Users:Edit";
            /// <summary>Permission to view user account lists and details.</summary>
            public const string UsersView = "Users:View";

            // Watchdog
            /// <summary>Permission to view watchdog monitoring status.</summary>
            public const string WatchdogView = "Watchdog:View";
        }

        /// <summary>
        /// Transforms a raw permission string into a standardized Authorization Policy name.
        /// </summary>
        /// <remarks>
        /// This follows the pattern: <c>"Category:Action"</c> becomes <c>"Can{Action}{Category}"</c>.
        /// <para>Example: <c>"Users:View"</c> returns <c>"CanViewUsers"</c>.</para>
        /// </remarks>
        /// <param name="permission">The permission constant from <see cref="Permissions"/>.</param>
        /// <returns>A formatted policy name string used by the ASP.NET Core Authorization system.</returns>
        public static string GetPolicyName(string permission)
        {
            if (string.IsNullOrEmpty(permission)) return string.Empty;

            var parts = permission.Split(':');
            var category = parts[0];
            var action = parts.Length > 1 ? parts[1] : string.Empty;

            return $"Can{action}{category}";
        }
    }
}
