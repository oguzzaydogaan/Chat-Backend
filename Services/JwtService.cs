using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        public JwtService(IConfiguration configuration, IOptionsMonitor<JwtBearerOptions> authenticationOptions)
        {
            _configuration = configuration;
            _authenticationOptions = authenticationOptions;
        }
        private readonly IConfiguration _configuration;
        private readonly IOptionsMonitor<JwtBearerOptions> _authenticationOptions;
        public LoginResponseDTO Authenticate(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!);
            var minutes = _configuration.GetValue<int>("JwtConfig:ExpirationInMinutes");
            var tokenExpiration = DateTime.UtcNow.AddMinutes(minutes);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("UserName", user.Name!),
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

            Console.WriteLine("TEST");
            return new LoginResponseDTO
            {
                Token = accessToken,
                Id = user.Id,
                ExpiresIn = tokenExpiration
            };
        }
        public SecurityToken Validate(string? token)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var parameters = _authenticationOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
            jwtSecurityTokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
            return validatedToken;
        }
    }
}
