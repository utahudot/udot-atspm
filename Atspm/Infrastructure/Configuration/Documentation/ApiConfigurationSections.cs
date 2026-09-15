#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration.Documentation/ApiConfigurationSections.cs
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

using System.ComponentModel.DataAnnotations;
using Utah.Udot.Atspm.Infrastructure.Configuration;

namespace Utah.Udot.Atspm.Infrastructure.Configuration.Documentation
{
    /// <summary>
    /// Configuration values used by the APIs to validate and issue JWT bearer tokens.
    /// </summary>
    [ConfigurationSection("Jwt", "Configuration for API JWT bearer authentication")]
    internal sealed class JwtConfiguration
    {
        /// <summary>
        /// Issuer value required when validating incoming JWT bearer tokens and issuing identity tokens.
        /// </summary>
        [Required]
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Audience value configured for JWT bearer token validation.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Symmetric signing key used to validate and issue JWT bearer tokens.
        /// </summary>
        [Required]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Number of days before identity API generated JWT bearer tokens expire.
        /// </summary>
        public double? ExpireDays { get; set; }
    }

    /// <summary>
    /// Optional OpenID Connect configuration used when an external identity provider is enabled.
    /// </summary>
    [ConfigurationSection("Oidc", "Configuration for optional OpenID Connect authentication")]
    internal sealed class OidcConfiguration
    {
        /// <summary>
        /// Authority URL for the OpenID Connect identity provider.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Client identifier registered with the OpenID Connect identity provider.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Client secret registered with the OpenID Connect identity provider.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Callback path used by the OpenID Connect redirect flow.
        /// </summary>
        public string CallbackPath { get; set; }
    }

    /// <summary>
    /// Configuration values used by the Config API GitHub release service.
    /// </summary>
    [ConfigurationSection(nameof(GitHubReleaseConfiguration), "Configuration for reading ATSPM GitHub releases")]
    internal sealed class GitHubReleaseConfiguration
    {
        /// <summary>
        /// User agent sent when calling the GitHub releases API.
        /// </summary>
        public string UserAgengt { get; set; }

        /// <summary>
        /// GitHub repository owner that contains ATSPM releases.
        /// </summary>
        public string RepositoryOwner { get; set; }

        /// <summary>
        /// GitHub repository name that contains ATSPM releases.
        /// </summary>
        public string RepositoryName { get; set; }
    }
}
