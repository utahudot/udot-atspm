using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Business.Tokens
{
    public class TokenService
    {
        private readonly IConfiguration configuration;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public TokenService(
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }


        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (user.Email == null)
            {
                throw new ArgumentNullException(nameof(user.Email));
            }
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            var roleNames = await signInManager.UserManager.GetRolesAsync(user);
            if (roleNames.Contains("Admin"))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                // Adding roleNames as claims
                foreach (var roleName in roleNames)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    var roleClaims = await roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
                    }
                }
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["Jwt:Issuer"],
                null,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}