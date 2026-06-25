#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Common/AtspmAuthorization.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace Utah.Udot.Atspm.Common
{
    //public interface IPluginHasController
    //{
    //    string ConfigRoute { get; }
    //}

    //public interface IPluginHasConfigController : IPluginHasController { }

    //public interface IPluginHasReportController : IPluginHasController { }

    //public interface IEventPublisher
    //{
    //    /// <summary>
    //    /// Publishes a standardized event to the Service Bus.
    //    /// </summary>
    //    /// <param name="type">The specific event name (e.g. atspm.hardware.offline)</param>
    //    /// <param name="subject">The status or specific target (e.g. Critical, Signal-102)</param>
    //    /// <param name="data">The specialized 'stuff' (Traffic info, Hardware stats, etc.)</param>
    //    /// <param name="extensions">Dictionary for extra telemetry/metadata</param>
    //    Task PublishAsync(string type, string subject, object data, IDictionary<string, object> extensions = null);
    //}


    /// <summary>
    /// Provides a centralized source of truth for ATSPM identity constants, roles, and functional permissions.
    /// This class handles the mapping between raw permission strings and authorization policy names.
    /// </summary>
    public static class AtspmAuthorization
    {
        /// <summary>
        /// The standard claim type URI used for role-based authorization.
        /// </summary>
        public const string RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public const string PluginFeatureClaimType = "atspm:plugin:feature";

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
            /// <summary>Administrator for managing devices</summary>
            public const string DeviceAdmin = "DeviceAdmin";
            /// <summary>Administrator for system-wide general configurations.</summary>
            public const string GeneralConfigurationAdmin = "GeneralConfigurationAdmin";
            /// <summary>Administrator for location-specific configurations.</summary>
            public const string LocationConfigurationAdmin = "LocationConfigurationAdmin";
            /// <summary>Administrator for managing plugins.</summary>
            public const string PluginAdmin = "PluginAdmin";
            /// <summary>Administrator for report generation and management.</summary>
            public const string ReportAdmin = "ReportAdmin";
            /// <summary>Administrator for managing security roles.</summary>
            public const string RoleAdmin = "RoleAdmin";
            /// <summary>Administrator for usage management.</summary>
            public const string UsageAdmin = "UsageAdmin";
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
            public const string ApiKeysCreate = "ApiKey:Create";
            /// <summary>Permission to view existing API keys.</summary>
            public const string ApiKeysView = "ApiKey:View";
            /// <summary>Permission to revoke active API keys.</summary>
            public const string ApiKeysRevoke = "ApiKey:Revoke";

            // Data
            /// <summary>Permission to modify ATSPM data records.</summary>
            public const string DataEdit = "Data:Edit";
            /// <summary>Permission to view ATSPM data records.</summary>
            public const string DataView = "Data:View";

            // Devices
            /// <summary>Permission to delete devices.</summary>
            public const string DeviceDelete = "Device:Delete";
            /// <summary>Permission to edit devices.</summary>
            public const string DeviceEdit = "Device:Edit";
            /// <summary>Permission to view devicess.</summary>
            public const string DeviceView = "Device:View";

            // General Configuration
            /// <summary>Permission to delete general system configurations.</summary>
            public const string GeneralConfigurationsDelete = "GeneralConfiguration:Delete";
            /// <summary>Permission to modify general system configurations.</summary>
            public const string GeneralConfigurationsEdit = "GeneralConfiguration:Edit";
            /// <summary>Permission to view general system configurations.</summary>
            public const string GeneralConfigurationsView = "GeneralConfiguration:View";

            // Location Configuration
            /// <summary>Permission to delete specific location configurations.</summary>
            public const string LocationConfigurationsDelete = "LocationConfiguration:Delete";
            /// <summary>Permission to modify specific location configurations.</summary>
            public const string LocationConfigurationsEdit = "LocationConfiguration:Edit";
            /// <summary>Permission to view specific location configurations.</summary>
            public const string LocationConfigurationsView = "LocationConfiguration:View";

            // Plugins


            // Reports
            /// <summary>Permission to access and view system reports.</summary>
            public const string ReportView = "Report:View";

            // Roles
            /// <summary>Permission to delete security roles.</summary>
            public const string RolesDelete = "Role:Delete";
            /// <summary>Permission to edit security role definitions.</summary>
            public const string RolesEdit = "Role:Edit";
            /// <summary>Permission to view security role definitions.</summary>
            public const string RolesView = "Role:View";

            // Usage
            /// <summary>Permission to delete usage reports.</summary>
            public const string UsageDelete = "Usage:Delete";
            /// <summary>Permission to edit usage reports.</summary>
            public const string UsageEdit = "Usage:Edit";
            /// <summary>Permission to view usage reports.</summary>
            public const string UsageView = "Usage:View";

            // Users
            /// <summary>Permission to delete user accounts.</summary>
            public const string UsersDelete = "User:Delete";
            /// <summary>Permission to edit user account details.</summary>
            public const string UsersEdit = "User:Edit";
            /// <summary>Permission to view user account lists and details.</summary>
            public const string UsersView = "User:View";

            // Watchdog
            /// <summary>Permission to view watchdog monitoring status.</summary>
            public const string WatchdogView = "Watchdog:View";
        }

        /// <summary>
        /// Maps each Role to the set of Permissions (Claims) it is authorized to hold.
        /// Used by the Identity Seeder to populate the database and by the app for verification.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, List<string>> RoleClaimsMap = new Dictionary<string, List<string>>
        {
            {
                Roles.Admin,
                new List<string> { Permissions.Admin }
            },
            {
                Roles.ApiKeyAdmin,
                new List<string>
                {
                    Permissions.ApiKeysCreate,
                    Permissions.ApiKeysView,
                    Permissions.ApiKeysRevoke
                }
            },
            {
                Roles.DataAdmin,
                new List<string>
                {
                    Permissions.DataView,
                    Permissions.DataEdit
                }
            },
            {
                Roles.DeviceAdmin,
                new List<string>
                {
                    Permissions.DeviceView,
                    Permissions.DeviceEdit,
                    Permissions.DeviceDelete
                }
            },
            {
                Roles.GeneralConfigurationAdmin,
                new List<string>
                {
                    Permissions.GeneralConfigurationsView,
                    Permissions.GeneralConfigurationsEdit,
                    Permissions.GeneralConfigurationsDelete,
                    Permissions.UsageView
                }
            },
            {
                Roles.LocationConfigurationAdmin,
                new List<string>
                {
                    Permissions.LocationConfigurationsView,
                    Permissions.LocationConfigurationsEdit,
                    Permissions.LocationConfigurationsDelete,
                    Permissions.DeviceView
                }
            },
            {
                Roles.ReportAdmin,
                new List<string>
                {
                    Permissions.ReportView
                }
            },
            {
                Roles.RoleAdmin,
                new List<string>
                {
                    Permissions.RolesView,
                    Permissions.RolesEdit,
                    Permissions.RolesDelete
                }
            },
            {
                Roles.UsageAdmin,
                new List<string>
                {
                    Permissions.UsageView,
                    Permissions.UsageEdit,
                    Permissions.UsageDelete,
                }
            },
            {
                Roles.UserAdmin,
                new List<string>
                {
                    Permissions.UsersView,
                    Permissions.UsersEdit,
                    Permissions.UsersDelete
                }
            },
            {
                Roles.WatchdogSubscriber,
                new List<string>
                {
                    Permissions.WatchdogView,
                    Permissions.ReportView
                }
            }
        };

        //public static readonly Dictionary<Type, List<string>> PluginInterfaceMap = new()
        //{
        //    // --- Data Ingest & Hardware Management ---
        //    { typeof(IEventLogDecoder),     new List<string> { Permissions.DataEdit } },
        //    { typeof(IEventLogImporter),    new List<string> { Permissions.DataEdit } },
        //    { typeof(IDeviceDownloader),    new List<string> { Permissions.DeviceEdit, Permissions.DataEdit } },
        //    { typeof(IDownloaderClient),    new List<string> { Permissions.DeviceView } },

        //    // --- Analytics & Metrics ---
        //    { typeof(IReportService),       new List<string> { Permissions.ReportView, Permissions.DataView } },
        //    { typeof(IWatchDogMetric),      new List<string> { Permissions.ReportView, Permissions.LogView } },
        //    { typeof(IPluginHasReportController), new List<string> { Permissions.ReportView } },

        //    // --- System & Background Operations ---
        //    { typeof(IScheduledTask),       new List<string> { Permissions.LogView } },
        //    { typeof(IBackgroundTask),      new List<string> { Permissions.LogView } },
        //    { typeof(IHealthCheck),          new List<string> { Permissions.LogView } },
        //    { typeof(IEventPublisher),      new List<string> { Permissions.LogView } },

        //    // --- Workflows ---
        //    { typeof(IWorkflowProvider),    new List<string> { Permissions.Admin, Permissions.DataEdit } },
        //    { typeof(IWorkflowStepProvider), new List<string> { Permissions.DataEdit } },

        //    // --- Security & Configuration ---
        //    { typeof(IAuthenticationProvider), new List<string> { Permissions.Admin } },
        //    { typeof(IConfigurablePlugin),     new List<string> { Permissions.Admin } },
        //    { typeof(IPluginHasConfigController), new List<string> { Permissions.Admin } },

        //    // --- UI/UX ---
        //    { typeof(IHasWebPages),         new List<string> { Permissions.DataView } }
        //};

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
