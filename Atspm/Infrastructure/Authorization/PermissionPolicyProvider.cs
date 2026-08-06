using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Utah.Udot.Atspm.Common;

namespace Utah.Udot.Atspm.Infrastructure.Authorization
{
    /// <summary>
    /// Dynamically generates authorization policies on-demand.
    /// Eliminates the need to pre-register policies on application startup.
    /// </summary>
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _backupProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionPolicyProvider"/> class.
        /// </summary>
        /// <param name="options">The authorization options.</param>
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _backupProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <inheritdoc />
        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var schemes = new[] { JwtBearerDefaults.AuthenticationScheme, "ApiKey", CookieAuthenticationDefaults.AuthenticationScheme };

            // CASE 1: Handle explicit requests for the "Admin" role policy
            if (policyName == AtspmAuthorization.Roles.Admin)
            {
                var adminPolicy = new AuthorizationPolicyBuilder(schemes)
                    .RequireClaim(AtspmAuthorization.RoleClaimType, AtspmAuthorization.Roles.Admin)
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(adminPolicy);
            }

            // CASE 2: Dynamically build permission-based policies on the fly where Policy Name is the Claim Value
            var permissionPolicy = new AuthorizationPolicyBuilder(schemes)
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(permissionPolicy);
        }

        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _backupProvider.GetDefaultPolicyAsync();

        /// <inheritdoc />
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _backupProvider.GetFallbackPolicyAsync();
    }
}
