using ERP.Core.Manager.Api.Application.Commons.Interfaces;
using ERP.Core.Manager.Api.Domain.Entities.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace ERP.Core.Manager.Api.Infrastructure.Services
{
    public class AuthServices(IConfiguration _config) : IAuthServices
    {
        public string GenerateAccessToken(User user, UserProfile profile, Role role)
        {
            var claims = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("user_id",      user.Id.ToString()),
                new("username",     user.UserName ?? ""),
                new("fullname",     user.Fullname ?? ""), 
                new("company_code", profile.CompanyId.ToString()),
            };

            var jwtKey = _config["Jwt:Key"] 
                ?? throw new InvalidOperationException("JWT Key not found in config.");
            
            var key =   new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
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
    }
}