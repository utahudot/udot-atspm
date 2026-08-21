#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Configuration/FederatedAuthenticationConfiguration.cs
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

namespace Utah.Udot.Atspm.Infrastructure.Configuration
{
    /// <summary>
    /// Configuration binding options for dynamic OIDC/OpenID Connect federated authentication.
    /// </summary>
    public class FederatedAuthenticationConfiguration
    {
        /// <summary>
        /// Gets or sets the list of configured federated identity providers.
        /// </summary>
        public List<FederatedProviderConfiguration> Providers { get; set; } = new();
    }

    /// <summary>
    /// Configuration for a specific external OIDC/SSO Identity Provider.
    /// </summary>
    public class FederatedProviderConfiguration
    {
        /// <summary>
        /// Gets or sets the unique name of the identity provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Gets or sets the authority URI of the OIDC identity provider.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the client identifier registered with the identity provider.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret registered with the identity provider.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the callback path where OIDC tokens are received.
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets custom scopes to request from the identity provider in addition to standard scopes.
        /// </summary>
        public List<string> CustomScopes { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether roles from the external identity provider should be synchronized to the local user.
        /// </summary>
        public bool EnableRoleSynchronization { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether geographic claims from the external provider should be synchronized.
        /// </summary>
        public bool EnableGeographySynchronization { get; set; } = true;

        /// <summary>
        /// Gets or sets the claim mappings that bind OIDC claims to local user profile fields.
        /// </summary>
        public UserProfileClaimConfiguration UserProfileClaims { get; set; } = new();
    }

    /// <summary>
    /// Maps incoming external OIDC Claims types to local User Profile properties.
    /// </summary>
    public class UserProfileClaimConfiguration
    {
        /// <summary>
        /// Gets or sets the claim name for the email address.
        /// </summary>
        public string Email { get; set; } = "email";

        /// <summary>
        /// Gets or sets the claim name for the user's first name.
        /// </summary>
        public string FirstName { get; set; } = "given_name";

        /// <summary>
        /// Gets or sets the claim name for the user's last name.
        /// </summary>
        public string LastName { get; set; } = "family_name";

        /// <summary>
        /// Gets or sets the claim name for the user's agency affiliation.
        /// </summary>
        public string Agency { get; set; } = "agency";

        /// <summary>
        /// Gets or sets the claim name containing assigned geographic area IDs.
        /// </summary>
        public string AreaIds { get; set; } = "atspm_areas";

        /// <summary>
        /// Gets or sets the claim name containing assigned geographic region IDs.
        /// </summary>
        public string RegionIds { get; set; } = "atspm_regions";

        /// <summary>
        /// Gets or sets the claim name containing assigned geographic jurisdiction IDs.
        /// </summary>
        public string JurisdictionIds { get; set; } = "atspm_jurisdictions";

        /// <summary>
        /// Gets or sets the claim name containing the user's roles.
        /// </summary>
        public string Roles { get; set; } = "roles";
    }
}
