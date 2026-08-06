using Microsoft.AspNetCore.Authorization;

namespace Utah.Udot.Atspm.Infrastructure.Authorization
{
    /// <summary>
    /// Represents a dynamic functional permission requirement.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the required functional permission string.
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
        /// </summary>
        /// <param name="permission">The permission identifier (e.g., "User:View").</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
