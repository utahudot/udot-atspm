using Microsoft.AspNetCore.Authorization;
using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Infrastructure.Attributes
{
    /// <summary>
    /// Automatically converts a permission constant (e.g., "Users:View") 
    /// into its corresponding policy name (e.g., "CanViewUsers").
    /// </summary>
    public class AuthorizePermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizePermissionAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor parses a permission string using the format "Category:Action" 
        /// and transforms it into a policy name following the pattern "Can{Action}{Category}".
        /// <example>
        /// Example: <c>"Users:View"</c> results in a policy requirement for <c>"CanViewUsers"</c>.
        /// </example>
        /// </remarks>
        /// <param name="permission">
        /// The raw permission string from <see cref="AtspmClaims.Permissions"/> (e.g., "Users:View").
        /// </param>
        public AuthorizePermissionAttribute(string permission)
        {
            if (string.IsNullOrEmpty(permission)) return;

            var parts = permission.Split(':');
            var category = parts[0];
            var action = parts.Length > 1 ? parts[1] : string.Empty;

            Policy = $"Can{action}{category}";
        }
    }
}
