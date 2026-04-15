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
