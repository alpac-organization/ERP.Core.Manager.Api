using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
using System.Security.Cryptography;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class AuthServices(IConfiguration _config) : IAuthServices
    {
        public string GenerateAccessToken(User user, string companyCode, Guid sessionId, IEnumerable<string> scopes)
        {
            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único del token
                new("user_id",      user.Id.ToString()),
                new("username",     user.UserName ?? ""),
                new("fullname",     user.Fullname ?? ""), 
                new("session_id",   sessionId.ToString()),
                new("company_code", companyCode),
            };

            // Agregamos cada módulo como un claim de tipo 'scopes'
            // Al haber varios con el mismo nombre, JWT los convierte en un array.
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scopes", scope));
            }

            var jwtKey = _config["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key not found in config.");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}