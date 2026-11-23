#region license
// Copyright 2026 Utah Departement of Transportation
// for IdentityApi - Utah.Udot.ATSPM.IdentityApi.Attributes/RejectApiKeyAuthenticationAttribute.cs
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Utah.Udot.ATSPM.IdentityApi.Attributes
{
    /// <summary>
    /// Rejects requests authenticated via API key, requiring a non-API-key authentication scheme such as JWT Bearer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RejectApiKeyAuthenticationAttribute : Attribute, IAuthorizationFilter
    {
        /// <inheritdoc/>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.HasClaim("AuthenticationMethod", "ApiKey"))
            {
                context.Result = new UnauthorizedObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "API key authentication is not permitted for API key management endpoints. Use a JWT Bearer token."
                });
            }
        }
    }
}
