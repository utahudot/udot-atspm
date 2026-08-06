#region license
// Copyright 2026 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Services.Identity.Dto/FederatedAuthDtos.cs
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

using System.Collections.Generic;

namespace Utah.Udot.Atspm.Services.Identity.Dto
{
    /// <summary>
    /// Data transfer object representing the challenge context needed to initiate an SSO redirect.
    /// </summary>
    public record ChallengePropertiesDto(
        string Scheme,
        string RedirectUri,
        IDictionary<string, string> Properties
    );

    /// <summary>
    /// Data transfer object holding security claims and provider keys returned from an external OIDC/SSO broker.
    /// </summary>
    public record ExternalIdentityDto(
        string ProviderName,
        string ProviderKey,
        IDictionary<string, string> UserClaims
    );

    /// <summary>
    /// Data transfer object representing the local authentication result of an SSO callback.
    /// </summary>
    public record FederatedLoginResponseDto(
        bool IsSuccess,
        string ErrorMessage,
        string Token,
        IEnumerable<string> AssignedPermissions
    );
}
