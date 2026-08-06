using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Infrastructure.Authorization
{
    /// <summary>
    /// Evaluates the user principal claims against dynamic functional permission requirements.
    /// Supports global bypass for the Admin role.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <inheritdoc />
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Global Bypass: System-wide Admin role is granted all permissions automatically
            if (context.User.IsInRole(AtspmAuthorization.Roles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Verify if the user possesses the specific functional permission claim
            var hasPermission = context.User.HasClaim(c => c.Type == AtspmAuthorization.RoleClaimType && c.Value == requirement.Permission);
            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
