#region license
// Copyright 2026 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Attributes/AuthorizePermissionAttribute.cs
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

using Microsoft.AspNetCore.Authorization;
using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Infrastructure.Attributes
{
    /// <summary>
    /// Custom authorization attribute that maps ATSPM permissions to dynamic policies.
    /// </summary>
    public class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionAttribute"/> class.
        /// </summary>
        /// <param name="permission">The permission constant from <see cref="AtspmAuthorization.Permissions"/>.</param>
        public AuthorizePermissionAttribute(string permission)
        {
            Policy = AtspmAuthorization.GetPolicyName(permission);
        }
    }
}
