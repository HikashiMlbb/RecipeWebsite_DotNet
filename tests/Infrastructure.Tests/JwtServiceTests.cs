// ReSharper disable InconsistentNaming

using System.IdentityModel.Tokens.Jwt;
using Domain.UserEntity;
using Infrastructure.Services;

namespace Infrastructure.Tests;

public class JwtServiceTests
{
    private readonly JwtDescriptorConfig _config;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _config = new JwtDescriptorConfig
        {
            Expires = null,
            Issuer = "TestIssuer",
            SigningKey = "SomeVeryLongSigningKey$$SomeVeryLongSigningKey$$SomeVeryLongSigningKey"
        };

        _jwtService = new JwtService(_config);
    }

    [Fact]
    public async Task SignTokenAsync_ValidUserId_ReturnsToken()
    {
        // Arrange
        var userId = new UserId(15);

        // Act
        var token = await _jwtService.SignTokenAsync(userId);
        var tokenHandler = new JwtSecurityTokenHandler();
        var validatedToken = tokenHandler.ReadJwtToken(token);
        var userIdClaim = validatedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(_config.Issuer, validatedToken.Issuer);
        Assert.True(validatedToken.ValidTo > DateTime.UtcNow);
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.Value.ToString(), userIdClaim.Value);
    }

    [Fact]
    public async Task SignTokenAsync_Expired()
    {
        // Arrange
        var userId = new UserId(15);
        var currentDateTime = DateTime.UtcNow + TimeSpan.FromDays(5);

        // Act
        var token = await _jwtService.SignTokenAsync(userId);
        var tokenHandler = new JwtSecurityTokenHandler();
        var validatedToken = tokenHandler.ReadJwtToken(token);
        var userIdClaim = validatedToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(_config.Issuer, validatedToken.Issuer);
        Assert.False(validatedToken.ValidTo > currentDateTime);
        Assert.NotNull(userIdClaim);
        Assert.Equal(userId.Value.ToString(), userIdClaim.Value);
    }
}