#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity/IFederatedAuthService.cs
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

using Utah.Udot.Atspm.Services.Identity.Dto;

namespace Utah.Udot.Atspm.Services.Identity
{
    /// <summary>
    /// Service contract for provider-agnostic Single Sign-On (SSO) handshakes, 
    /// OAuth/OIDC external profile challenge mapping, and local account linkage.
    /// </summary>
    public interface IFederatedAuthService
    {
        /// <summary>
        /// configures the challenge properties and authentication properties for initiating an external OIDC handshake.
        /// </summary>
        /// <param name="providerName">The name of the external identity provider (e.g. "Authentik", "UtahID").</param>
        /// <param name="redirectUri">The local callback path to process after provider handshakes complete.</param>
        /// <returns>A task representing the asynchronous operation, containing challenge configurations.</returns>
        Task<ChallengePropertiesDto> PrepareChallengeAsync(string providerName, string redirectUri);

        /// <summary>
        /// Processes external authentication callbacks, mapping claims, and returning an ATSPM identity token.
        /// </summary>
        /// <param name="providerName">The name of the external provider.</param>
        /// <param name="externalInfo">The identity model returned from the provider.</param>
        /// <returns>A task representing the asynchronous operation, containing the local login results.</returns>
        Task<FederatedLoginResponseDto> HandleCallbackAsync(string providerName, ExternalIdentityDto externalInfo);

        /// <summary>
        /// Links an existing local user account to an external Single Sign-On credentials context.
        /// </summary>
        /// <param name="userId">The local system user ID.</param>
        /// <param name="externalInfo">The SSO provider claims model.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LinkAccountAsync(string userId, ExternalIdentityDto externalInfo);
    }
}
