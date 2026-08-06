#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.LogMessages.Identity/FederatedAuthServiceLogMessages.cs
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

using Microsoft.Extensions.Logging;

namespace Utah.Udot.Atspm.Infrastructure.LogMessages.Identity
{
    /// <summary>
    /// Provides strongly-typed high-performance logging methods for federated OIDC/SSO handshakes and callbacks.
    /// </summary>
    public partial class FederatedAuthServiceLogMessages(ILogger logger)
    {
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Logs when an external SSO challenge properties setup begins.
        /// </summary>
        /// <param name="provider">The name of the external SSO provider.</param>
        /// <param name="redirectUri">The local callback redirect URI.</param>
        [LoggerMessage(EventId = 1181, EventName = "SSO Challenge Configured", Level = LogLevel.Information, Message = "Configuring challenge authentication properties for external SSO provider: '{Provider}' (Callback Redirect URI: '{RedirectUri}').")]
        public partial void ChallengePrepared(string provider, string redirectUri);

        /// <summary>
        /// Logs a processed external callback.
        /// </summary>
        /// <param name="provider">The name of the external SSO provider.</param>
        /// <param name="email">The email resolved from OIDC claims.</param>
        [LoggerMessage(EventId = 1182, EventName = "SSO Callback Processed", Level = LogLevel.Information, Message = "SSO Callback successfully processed from '{Provider}' for user: '{Email}'. Mapped to local account.")]
        public partial void SSOCallbackProcessed(string provider, string email);

        /// <summary>
        /// Logs a failed external callback.
        /// </summary>
        /// <param name="provider">The name of the external SSO provider.</param>
        /// <param name="error">The validation error message.</param>
        [LoggerMessage(EventId = 1183, EventName = "SSO Handshake Failed", Level = LogLevel.Warning, Message = "SSO callback and credentials handshake failed for provider '{Provider}'. Error: {Error}")]
        public partial void SSOCallbackFailed(string provider, string error);

        /// <summary>
        /// Logs when a local user links an external OIDC login provider.
        /// </summary>
        /// <param name="provider">The name of the external SSO provider.</param>
        /// <param name="userId">The local user identifier.</param>
        [LoggerMessage(EventId = 1184, EventName = "External SSO Linked", Level = LogLevel.Information, Message = "Linked external authentication provider '{Provider}' successfully to local user ID: '{UserId}'")]
        public partial void AccountLinked(string provider, string userId);

        /// <summary>
        /// Logs when a new user account is dynamically created upon first external SSO login.
        /// </summary>
        /// <param name="provider">The name of the external SSO provider.</param>
        /// <param name="email">The email of the newly generated account.</param>
        /// <param name="userId">The unique identifier assigned to the new user.</param>
        [LoggerMessage(EventId = 1185, EventName = "SSO Auto-Provision Complete", Level = LogLevel.Information, Message = "Dynamically auto-provisioned user account '{Email}' (ID: {UserId}) via OIDC SSO provider '{Provider}' upon first successful login.")]
        public partial void NewSSOUserCreated(string provider, string email, string userId);

        /// <summary>
        /// Logs when a user's system roles are dynamically synchronized from OIDC claims.
        /// </summary>
        /// <param name="email">The email address of the synchronized user account.</param>
        /// <param name="roles">The comma-separated active system roles assigned to the user.</param>
        [LoggerMessage(EventId = 1186, EventName = "SSO Roles Synchronized", Level = LogLevel.Information, Message = "Dynamically synchronized system roles from OIDC SSO provider for user '{Email}'. Active roles: [{Roles}]")]
        public partial void RolesSynchronized(string email, string roles);

        /// <summary>
        /// Logs when a user's geographic profile and boundaries are dynamically synchronized from OIDC claims.
        /// </summary>
        /// <param name="email">The email address of the synchronized user account.</param>
        /// <param name="areas">The comma-separated geographic area identifiers.</param>
        /// <param name="regions">The comma-separated geographic region identifiers.</param>
        /// <param name="jurisdictions">The comma-separated geographic jurisdiction identifiers.</param>
        [LoggerMessage(EventId = 1187, EventName = "SSO Geography Synchronized", Level = LogLevel.Information, Message = "Dynamically synchronized geographic boundaries from OIDC SSO provider for user '{Email}'. Areas: [{Areas}], Regions: [{Regions}], Jurisdictions: [{Jurisdictions}]")]
        public partial void GeographySynchronized(string email, string areas, string regions, string jurisdictions);
    }
}
