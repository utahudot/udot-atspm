using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Business.Tokens
{
    public class TokenService
    {
        private readonly string _secretKey;
        private readonly int tokenExpireTime = 12 * 60;

        public TokenService(string secretKey)
        {
            _secretKey = secretKey;
        }

        public string GenerateToken(string userId, string[] role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var roleClaims = new List<Claim>();
            foreach (var roleClaim in role)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, roleClaim));
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
            claims = claims.Union(roleClaims).ToArray();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpireTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;
            return tokenHandler.WriteToken(token);
        }
    }
}