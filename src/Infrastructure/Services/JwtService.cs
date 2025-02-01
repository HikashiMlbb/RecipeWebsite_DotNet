using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Users.Services;
using Domain.UserEntity;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtDescriptorConfig _config;

    public JwtService(JwtDescriptorConfig config)
    {
        _config = config;
    }

    public async Task<string> SignTokenAsync(UserId foundUserId)
    {
        return await Task.Run(() =>
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, foundUserId.Value.ToString()) };
            var key = new SigningCredentials(_config.GetKey(), SecurityAlgorithms.HmacSha384);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow + (_config.Expires ?? TimeSpan.FromHours(1)),
                Audience = _config.Audience,
                Issuer = _config.Issuer,
                SigningCredentials = key
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        });
    }
}