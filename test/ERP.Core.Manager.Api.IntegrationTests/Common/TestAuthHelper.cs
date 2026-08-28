using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace ERP.Core.Manager.Api.IntegrationTests.Common
{
    public static class TestAuthHelper
    {
        public static string CreateBearerToken(string jwtKey, Guid userId)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new("user_id", userId.ToString()),
                new("username", "test.user"),
                new("fullname", "Test User"),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "ERP.Core.Manager.Api",
                audience: "ERP.Clients.Web",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
