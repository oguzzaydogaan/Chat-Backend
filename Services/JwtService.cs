using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories.DTOs;
using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public LoginResponseDTO Authenticate(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!);
            var minutes = _configuration.GetValue<int>("JwtConfig:ExpirationInMinutes");
            var tokenExpiration = DateTime.UtcNow.AddMinutes(minutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim(JwtRegisteredClaimNames.Name, user.Email!),
                        new Claim("Time",tokenExpiration.ToString())
                    }),
                Expires = tokenExpiration,
                Issuer = _configuration["JwtConfig:Issuer"],
                Audience = _configuration["JwtConfig:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);
            return new LoginResponseDTO
            {
                Token = accessToken,
                Id = user.Id,
                ExpiresIn = tokenExpiration
            };
        }
    }
}
